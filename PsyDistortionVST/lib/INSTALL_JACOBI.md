# Инструкция по установке Jacobi.Vst.NET

Положи следующие DLL файлы в эту папку:

## Необходимые файлы:
- `Jacobi.Vst.Core.dll`
- `Jacobi.Vst.Framework.dll`
- `Jacobi.Vst.Interop.dll`

## Где скачать:

### Вариант 1 - Официальный сайт (рекомендуется)
- Перейди на: https://github.com/obiwanjacobi/vst.net
- Скачай последний Release
- Распакуй архив и найди нужные DLL в папке bin

### Вариант 2 - NuGet (если доступно)
```
Install-Package Jacobi.Vst.Framework
```

### Вариант 3 - Старый CodePlex (архив)
- https://archive.codeplex.com/?p=vstnet
- Скачай Source Code или Binaries

## Проверка:
После копирования файлов в эту папку, запусти `build.bat` из корня проекта.

## Важно:
- Используй версию для .NET Framework 4.7.2
- Проверь совместимость архитектуры (x86/x64/AnyCPU)
- Все DLL должны быть из одной версии Jacobi.Vst.NET

## Структура папки после установки:
```
lib/
├── Jacobi.Vst.Core.dll
├── Jacobi.Vst.Framework.dll
├── Jacobi.Vst.Interop.dll
└── INSTALL_JACOBI.md (этот файл)
```