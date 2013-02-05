param($installPath, $toolsPath, $package, $project)

$msbuild = [Microsoft.Build.Evaluation.ProjectCollection]::GlobalProjectCollection.GetLoadedProjects($project.FullName) | Select-Object -First 1


$importToRemoves = $msbuild.Xml.Imports | Where-Object { $_.Project.Endswith('Microsoft.TextTemplating.targets') }

foreach($import in $importToRemoves)
{
    $msbuild.Xml.RemoveChild($import) | out-null
}

$propertyGroupToRemove = $msbuild.Xml.PropertyGroups | Where-Object { $_.Label -eq 'OrgJuippT4Properties' } | Select-Object -First 1
$msbuild.Xml.RemoveChild($propertyGroupToRemove) | out-null

$project.Save()

Write-Host "Removed Imports for T4"
Write-Host "Removed Property Group > OrgJuippT4Properties"




