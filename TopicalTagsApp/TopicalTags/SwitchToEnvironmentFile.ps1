param (

    [Parameter(Position = 0, Mandatory = $true)]
    [string]
    $fileName,
    [Parameter(Position = 1, Mandatory = $true)]
    [string]
    $configuration
)

Write-Output "Switching file into other environment"
Write-Output "scriptName = $fileName"
Write-Output "configuration = $configuration"

$fileBase = [System.IO.Path]::GetFileNameWithoutExtension($fileName)
$fileExtension = [System.IO.Path]::GetExtension($fileName)
$fileBaseBackup = [System.IO.Path]::GetTempFileName()
$workingDirectory = [System.IO.Path]::GetDirectoryName($fileName)
$configurationSpecificFile = [System.IO.Path]::Combine($workingDirectory, "$fileBase.$configuration$fileExtension")

Write-Output "Configuration Specific File: $configurationSpecificFile"
Write-Output "Backup of Default file at: $fileBaseBackup"

try {

    if (Test-Path $configurationSpecificFile) {
		if(Test-Path $fileName)
		{
			Write-Output "Backing $fileName"
			Move-Item $fileName $fileBaseBackup -Force
		}
        Copy-Item $configurationSpecificFile $fileName
    }
    else {
        Write-Output "Configuration specific file '$configurationSpecificFile' does not exists. No substitution will be done"
    }
}
catch {
    $ex = $_.Exception.Message
    Write-Output "Ended with errors:"
    Write-Output $ex
    Write-Output "Reverting file: $fileName"
    Move-Item $fileBaseBackup $fileName -Force
}
finally {
    if (Test-Path $fileBaseBackup) {
        Remove-Item $fileBaseBackup
    }
}
