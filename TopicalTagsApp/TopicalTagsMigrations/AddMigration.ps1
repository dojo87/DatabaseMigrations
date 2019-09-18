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

$outputPath = Get-MigrationFilePath -migrationName $migration -migrationsLocation $migrationsFolder

# Run migration generation
$sqlPackageExe = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"
&$sqlPackageExe /Action:Script `
	/SourceFile:"../TopicalTags/bin/Debug/TopicalTags.dacpac" `
	/Profile:../TopicalTags/TopicalTags.publish.xml `
	/OutputPath:$outputPath