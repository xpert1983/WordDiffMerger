@echo off
echo Установка NuGet пакетов...
nuget restore WordDiffMerger.sln

echo Сборка проекта...
msbuild WordDiffMerger.sln /p:Configuration=Release /p:Platform="Any CPU"

echo Готово! Исполняемый файл: bin\Release\WordDiffMerger.exe
pause
