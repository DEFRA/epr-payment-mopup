CREATE DATABASE MopupLocal;
GO
USE MopupLocal;
GO
CREATE SCHEMA Lookup;
GO
CREATE TABLE Lookup.PaymentStatus (Id int PRIMARY KEY, Status varchar(20) NOT NULL);
INSERT Lookup.PaymentStatus VALUES (0,'Initiated'),(1,'InProgress'),(2,'Success'),(3,'Failed'),(4,'Error'),(5,'UserCancelled');
CREATE TABLE dbo.Payment (
    Id int IDENTITY PRIMARY KEY, UserId uniqueidentifier NOT NULL, ExternalPaymentId uniqueidentifier NOT NULL DEFAULT NEWID(),
    InternalStatusId int NOT NULL REFERENCES Lookup.PaymentStatus(Id), Regulator varchar(20) NOT NULL,
    Reference nvarchar(255) NOT NULL, Amount decimal(19,4) NOT NULL, ReasonForPayment nvarchar(255) NOT NULL,
    CreatedDate datetime2 NOT NULL, UpdatedByUserId uniqueidentifier NOT NULL, UpdatedDate datetime2 NOT NULL
);
CREATE TABLE dbo.OnlinePayment (
    Id int IDENTITY PRIMARY KEY, PaymentId int NOT NULL UNIQUE REFERENCES dbo.Payment(Id), OrganisationId uniqueidentifier NOT NULL,
    GovPayPaymentId varchar(50) NULL, GovPayStatus varchar(20) NULL, ErrorCode varchar(255) NULL,
    ErrorMessage varchar(255) NULL, UpdatedByOrgId uniqueidentifier NOT NULL
);
DECLARE @cases TABLE (Name varchar(50), AgeMinutes int, Status int, GovPayId varchar(50));
INSERT @cases VALUES
 ('success',200,1,'success'),('failed',200,1,'failed'),('cancelled',200,1,'cancelled'),('error',200,1,'error'),
 ('nonfinal',200,1,'nonfinal'),('http-error',200,1,'http-error'),('malformed',200,1,'malformed'),
 ('missing-state',200,1,'missing-state'),('after-error',200,1,'after-error'),
 ('initiated',200,0,'should-not-call-initiated'),('recent',170,1,'should-not-call-recent'),
 ('too-old',280,1,'should-not-call-old'),('no-id',200,1,''),('null-id',200,1,NULL),
 ('already-final',200,2,'should-not-call-final');
INSERT dbo.Payment (UserId,InternalStatusId,Regulator,Reference,Amount,ReasonForPayment,CreatedDate,UpdatedByUserId,UpdatedDate)
SELECT NEWID(),Status,'GB-ENG',Name,10,'Local regression fixture',DATEADD(minute,-AgeMinutes,SYSUTCDATETIME()),NEWID(),'2000-01-01' FROM @cases;
INSERT dbo.OnlinePayment (PaymentId,OrganisationId,GovPayPaymentId,GovPayStatus,UpdatedByOrgId)
SELECT p.Id,NEWID(),c.GovPayId,'original',NEWID() FROM dbo.Payment p JOIN @cases c ON c.Name=p.Reference;
GO
