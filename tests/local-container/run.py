#!/usr/bin/env python3
"""Build and exercise disposable local fixtures. Always remove this Compose project."""
from pathlib import Path
import json
import subprocess
import time

ROOT = Path(__file__).resolve().parents[2]
COMPOSE = ['docker', 'compose', '-p', 'mopup-net10-local', '-f', str(Path(__file__).with_name('compose.yaml'))]
SQL = ['exec', '-T', 'sql', '/opt/mssql-tools18/bin/sqlcmd', '-S', 'localhost', '-U', 'sa', '-P',
       'LocalMopup_TestOnly42!', '-C', '-b']
ARTIFACTS = ROOT / 'tests/local-container/artifacts'


def compose(*args, **kwargs):
    result = subprocess.run(COMPOSE + list(args), text=True, capture_output=True, **kwargs)
    if result.returncode:
        raise RuntimeError(result.stdout + result.stderr)
    return result.stdout


def snapshot():
    query = """SET NOCOUNT ON; SELECT p.Reference,p.InternalStatusId,o.GovPayStatus,
        COALESCE(o.ErrorCode,'-'),COALESCE(o.ErrorMessage,'-'),
        CASE WHEN p.UpdatedDate > '2000-01-01' THEN 1 ELSE 0 END
        FROM MopupLocal.dbo.Payment p JOIN MopupLocal.dbo.OnlinePayment o ON o.PaymentId=p.Id ORDER BY p.Id"""
    output = compose(*SQL, '-Q', query, '-h', '-1', '-s', '|', '-W', '-w', '65535')
    return {parts[0]: parts[1:] for line in output.splitlines() if '|' in line
            for parts in [[s.strip() for s in line.split('|')]]}


def calls():
    return json.loads(compose('exec', '-T', 'govpay', 'python', '-c',
        "import urllib.request;print(urllib.request.urlopen('http://localhost:8080/calls').read().decode())"))


def main():
    ARTIFACTS.mkdir(exist_ok=True)
    (ARTIFACTS / 'results.json').unlink(missing_ok=True)
    try:
        subprocess.run(['docker', 'build', '--platform', 'linux/amd64', '-t',
                        'epr-payment-mopup:net10-local-test', '-f',
                        str(ROOT / 'src/EPR.Payment.Mopup.Function/Dockerfile'), str(ROOT / 'src')], check=True)
        print(compose('up', '-d', '--wait', '--wait-timeout', '240', 'sql', 'storage', 'govpay'), flush=True)
        compose(*SQL, input=Path(__file__).with_name('seed.sql').read_text())
        print(compose('up', '-d', 'function'), flush=True)
        deadline = time.monotonic() + 150
        while time.monotonic() < deadline:
            rows = snapshot()
            counts = calls()
            logs = compose('logs', '--no-color', 'function')
            if (rows.get('after-error', [''])[0] == '2' and counts.get('nonfinal', 0) >= 2
                    and logs.count("Executed 'Functions.MopUpPaymentsFunction' (Succeeded,") >= 2):
                break
            time.sleep(3)
        else:
            raise AssertionError('Timed out waiting for two successful timer cycles; inspect artifacts/function.log')

        expected = {
            'success': ['2', 'Success', '-', '-', '1'],
            'failed': ['3', 'Failed', 'P0010', 'Declined', '1'],
            'cancelled': ['5', 'UserCancelled', 'P0030', 'User cancelled', '1'],
            'error': ['4', 'Error', 'P0050', 'Payment expired', '1'],
            'after-error': ['2', 'Success', '-', '-', '1'],
        }
        for name, value in expected.items():
            assert rows[name] == value, (name, rows[name], value)
            assert counts[name] == 1, (name, 'final payment was processed again', counts)
        for name in ['nonfinal', 'http-error', 'malformed', 'missing-state', 'recent', 'too-old', 'no-id', 'null-id']:
            assert rows[name] == ['1', 'original', '-', '-', '0'], (name, rows[name])
        assert rows['initiated'] == ['0', 'original', '-', '-', '0'], rows['initiated']
        assert rows['already-final'] == ['2', 'original', '-', '-', '0'], rows['already-final']
        assert len(rows) == 15, rows
        assert not any(key.startswith('should-not-call') for key in counts), counts
        for name in ['nonfinal', 'http-error', 'malformed', 'missing-state']:
            assert counts[name] >= 2, (name, 'not retried next cycle', counts)
        logs = compose('logs', '--no-color', 'function')
        assert 'Worker process started and initialized' in logs, 'Worker initialization missing'
        assert 'MopUpPaymentsFunction' in logs, 'Timer discovery missing'
        assert 'Mop Up time trigger function executed' in logs, 'Information logging missing'
        assert logs.count("Executed 'Functions.MopUpPaymentsFunction' (Succeeded,") >= 2, 'Timer completion missing'
        assert 'fail: Function.MopUpPaymentsFunction.User' in logs, 'Expected fixture-error logging missing'
        runtime = compose('exec', '-T', 'function', 'dotnet', '--list-runtimes')
        assert 'Microsoft.NETCore.App 10.' in runtime, runtime
        assert compose('exec', '-T', 'function', 'id', '-un').strip() == 'dotnet'
        report = {'result': 'PASS', 'fixtures': 15, 'rows': rows, 'govpay_calls': counts, 'runtime': runtime,
                  'assertions': ['worker startup', 'timer discovery and two cycles', 'non-root user',
                                 'JSON configuration and environment override', 'Information/error logs', 'SQL selection and writes', 'status/error mappings',
                                 'unchanged excluded/error rows', 'retries and no terminal-row reprocessing']}
        (ARTIFACTS / 'results.json').write_text(json.dumps(report, indent=2) + '\n')
        print(json.dumps(report, indent=2), flush=True)
    finally:
        for service in ['function', 'sql', 'storage', 'govpay']:
            result = subprocess.run(COMPOSE + ['logs', '--no-color', service], text=True, capture_output=True)
            (ARTIFACTS / f'{service}.log').write_text(result.stdout + result.stderr)
        print(compose('down', '--volumes', '--remove-orphans'), flush=True)


if __name__ == '__main__':
    main()
