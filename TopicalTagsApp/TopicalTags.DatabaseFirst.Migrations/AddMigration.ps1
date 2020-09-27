param (
    [Parameter(Position = 0, Mandatory = $true)]
    [string]
    $migration,
    [string]
    $migrationsFolder = "./Migrations"
)

function Get-MigrationFilePath()
{
    param([string]$migrationName, [string]$migrationsLocation)

    # Locate folder with migrations
	if(-not [System.IO.Path]::IsPathRooted($migrationsLocation)){
        $location = Get-Location
        $migrationsLocation = [System.IO.Path]::Combine($location, $migrationsLocation)
    }

    # Find highest number prefix in existing migrations
	$allMigrations = [System.IO.Directory]::GetFiles($migrationsLocation, "*.sql")

	$maxMigration = 0;
	foreach($existingMigration in $allMigrations)
	{
		$existingMigrationName = [System.IO.Path]::GetFileNameWithoutExtension($existingMigration)
		$existingMigrationNumber = 0;
		if([int]::TryParse($existingMigrationName.Substring(0,4).TrimStart('0'),  [ref] $existingMigrationNumber)){
			$maxMigration = [Math]::Max($maxMigration, $existingMigrationNumber)
		}
	}

    # Generate a name for the new migration with a higher prefix.
	$migrationNumberValue = ($maxMigration+1).ToString().PadLeft(4,'0')
	$migrationFile = [System.IO.Path]::Combine($migrationsLocation,"$migrationNumberValue-$migrationName.sql")

	return $migrationFile
}
Write-Output "Determinig migration number"
$outputPath = Get-MigrationFilePath -migrationName $migration -migrationsLocation $migrationsFolder

Write-Output "Migration Path: $outputPath"
Write-Output "Generating migration"

# Run migration generation
$sqlPackageExe = $Env:SqlPackage
Write-Output $sqlPackageExe
&$sqlPackageExe /Action:Script `
	/SourceFile:"../TopicalTags.DatabaseFirst.MigrationsModel/bin/Debug/TopicalTags.DatabaseFirst.MigrationsModel.dacpac" `
	/Profile:../TopicalTags.DatabaseFirst.MigrationsModel/TopicalTags.publish.xml `
	/TargetDatabaseName:"TopicalTags.DatabaseFirstTransitions" `
	/OutputPath:$outputPath