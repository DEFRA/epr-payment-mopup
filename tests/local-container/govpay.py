"""Deterministic local GOV.UK Pay fixture; never forwards requests externally."""
from collections import Counter
from http.server import BaseHTTPRequestHandler, HTTPServer
import json

calls = Counter()
states = {
    'success': {'status': 'success', 'finished': True},
    'failed': {'status': 'failed', 'finished': True, 'code': 'P0010', 'message': 'Declined'},
    'cancelled': {'status': 'failed', 'finished': True, 'code': 'P0030', 'message': 'User cancelled'},
    'error': {'status': 'error', 'finished': True, 'code': 'P0050', 'message': 'Payment expired'},
    'nonfinal': {'status': 'created', 'finished': False},
    'after-error': {'status': 'success', 'finished': True},
}

class Handler(BaseHTTPRequestHandler):
    def do_GET(self):
        if self.path == '/calls':
            return self.reply(200, dict(calls))
        if not self.path.startswith('/v1/payments/'):
            return self.reply(404, {'error': 'Unexpected endpoint; check environment override'})
        if self.headers.get('Authorization') != 'Bearer local-test-token':
            return self.reply(401, {'error': 'Missing fixture bearer token'})
        payment_id = self.path.rstrip('/').split('/')[-1]
        calls[payment_id] += 1
        if payment_id == 'http-error':
            return self.reply(500, {'error': 'Fixture failure'})
        if payment_id == 'malformed':
            self.send_response(200)
            self.end_headers()
            self.wfile.write(b'not-json')
            return
        if payment_id == 'missing-state':
            return self.reply(200, {'paymentId': payment_id})
        if payment_id not in states:
            return self.reply(404, {'error': 'Unexpected payment selected'})
        return self.reply(200, {'paymentId': payment_id, 'state': states[payment_id]})

    def reply(self, code, body):
        self.send_response(code)
        self.send_header('Content-Type', 'application/json')
        self.end_headers()
        self.wfile.write(json.dumps(body).encode())

HTTPServer(('0.0.0.0', 8080), Handler).serve_forever()
