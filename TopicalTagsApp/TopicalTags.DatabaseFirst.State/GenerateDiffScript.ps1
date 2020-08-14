param (
	[Parameter(Mandatory = $true)]
    [string]
    $profile,
    [Parameter(Mandatory = $true)]
	[string]
    $dacpac,
	[string]
    $output
)
$sqlPackageExe = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"

if($output -eq '' -or $output -eq $null){
    $output = $profile.Replace(".xml",".sql")
}

&$sqlPackageExe /Action:Script  /SourceFile:"$dacpac" /Profile:"$profile" /OutputPath:"$output"