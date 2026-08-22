# export-solution.ps1
# Запускать из корня решения (там где .sln)

param(
    [string]$OutputFile = "solution-dump.md",
    [string[]]$IncludeExtensions = @("*.cs", "*.csproj", "*.sln", "*.json", "*.config", "*.xaml", "*.xaml.cs"),
    [string[]]$ExcludeDirs = @("bin", "obj", ".vs", "node_modules", "packages", ".git", "TestResults")
)

$root = Get-Location
Write-Host "Сканирую решение в: $($root.Path)" -ForegroundColor Cyan

# Очищаем/создаём выходной файл
"# Solution Dump: $($root.Path.Split('\')[-1])" | Out-File $OutputFile -Encoding utf8
"Сгенерировано: $(Get-Date -Format 'yyyy-MM-dd HH:mm')" | Out-File $OutputFile -Append -Encoding utf8
"" | Out-File $OutputFile -Append -Encoding utf8

# Функция проверки, находится ли путь в исключённой папке
function Test-Excluded($fullPath) {
    foreach ($dir in $ExcludeDirs) {
        if ($fullPath -match "\\$dir\\") { return $true }
    }
    return $false
}

# 1. Строим дерево каталогов
"## Структура проекта" | Out-File $OutputFile -Append -Encoding utf8
"````" | Out-File $OutputFile -Append -Encoding utf8

Get-ChildItem -Recurse -Directory | Where-Object {
    -not (Test-Excluded $_.FullName)
} | Sort-Object FullName | ForEach-Object {
    $depth = ($_.FullName.Replace($root.Path, '') -split '\\').Count - 1
    $indent = "  " * $depth
    "$indent- $($_.Name)/" | Out-File $OutputFile -Append -Encoding utf8
}

"````" | Out-File $OutputFile -Append -Encoding utf8
"" | Out-File $OutputFile -Append -Encoding utf8

# 2. Собираем список файлов по расширениям
$files = foreach ($ext in $IncludeExtensions) {
    Get-ChildItem -Recurse -Filter $ext -File
}
$files = $files | Where-Object { -not (Test-Excluded $_.FullName) } | Sort-Object FullName -Unique

Write-Host "Найдено файлов: $($files.Count)" -ForegroundColor Cyan

# 3. Пишем содержимое каждого файла
"## Содержимое файлов" | Out-File $OutputFile -Append -Encoding utf8
"" | Out-File $OutputFile -Append -Encoding utf8

$counter = 0
foreach ($file in $files) {
    $counter++
    $relPath = $file.FullName.Replace($root.Path, '').TrimStart('\')
    Write-Progress -Activity "Экспорт файлов" -Status $relPath -PercentComplete (($counter / $files.Count) * 100)

    $lang = switch ($file.Extension) {
        ".cs"      { "csharp" }
        ".csproj"  { "xml" }
        ".sln"     { "text" }
        ".json"    { "json" }
        ".config"  { "xml" }
        ".xaml"    { "xml" }
        default    { "text" }
    }

    "### $relPath" | Out-File $OutputFile -Append -Encoding utf8
    "``````$lang" | Out-File $OutputFile -Append -Encoding utf8
    Get-Content $file.FullName -Raw | Out-File $OutputFile -Append -Encoding utf8
    "``````" | Out-File $OutputFile -Append -Encoding utf8
    "" | Out-File $OutputFile -Append -Encoding utf8
}

Write-Host "Готово! Результат: $OutputFile" -ForegroundColor Green
$sizeKb = [math]::Round((Get-Item $OutputFile).Length / 1KB, 1)
Write-Host "Размер файла: $sizeKb KB" -ForegroundColor Yellow