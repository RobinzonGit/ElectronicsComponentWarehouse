# Electronics Component Warehouse Management System

## Описание
Клиент-серверное приложение для управления складом электронных компонентов с использованием современных технологий .NET.

## Технологический стек
- **Backend**: ASP.NET Core Web API (.NET 8)
- **Frontend**: WPF (.NET 8) с MVVM
- **Database**: SQL Server + Entity Framework Core
- **Authentication**: JWT Bearer Tokens
- **Logging**: Serilog

## Структура решения
ElectronicsComponentWarehouse/
├── src/
│ ├── Domain/ # Сущности, интерфейсы домена
│ ├── Application/ # Бизнес-логика, DTO, сервисы
│ ├── Infrastructure/ # Реализация инфраструктуры
│ │ ├── Data/ # Data Access (EF Core)
│ │ └── Identity/ # Аутентификация
│ ├── Web.API/ # ASP.NET Core Web API
│ └── Desktop.Client/ # WPF приложение
├── tests/ # Тесты
├── docs/ # Документация
└── scripts/ # Скрипты для развертывания

text

## Быстрый старт
1. Установите .NET 8 SDK и SQL Server
2. Клонируйте репозиторий
3. Восстановите зависимости: `dotnet restore`
4. Настройте строку подключения в appsettings.json
5. Примените миграции: `dotnet ef database update`
6. Запустите сервер: `dotnet run --project src/Web.API`
7. Запустите клиент: `dotnet run --project src/Desktop.Client`

## Лицензия
MIT
