$files = Get-ChildItem "RayLibAutoChess/src" -Recurse -Include "*.cs"

foreach ($file in $files) {
    $content = Get-Content $file.FullName
    $filtered = $content | Where-Object { -not $_.Trim().StartsWith("//") }
    Set-Content $file.FullName $filtered
}

Write-Host "All comments removed from source files."
