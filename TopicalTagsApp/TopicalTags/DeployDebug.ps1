$sqlPackageExe = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"
&$sqlPackageExe /Action:Publish  /SourceFile:"./bin/Debug/TopicalTags.dacpac" /Profile:TopicalTags.publish.xml