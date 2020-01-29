/*
Post-Deployment Script Template
This file will be taken as default if no environment specific script will be found. 
You should not edit PostScript.sql as it is generated. If you want to edit default values, edit PostScript.Default.sql
*/

:r .\TagConfiguration.sql	
:r .\TestData.sql
:r .\Configuration\Default.sql