cd %~dp0
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\installutil.exe Truking.CRM.WinSrv
Net Start Truking.CRM.WinSrv
sc config Truking.CRM.WinSrv start= auto
pause