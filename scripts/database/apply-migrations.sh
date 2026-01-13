#!/bin/bash

# Скрипт для применения миграций базы данных

echo "Применение миграций базы данных..."

# Установка переменных окружения
export ASPNETCORE_ENVIRONMENT=Development
export DOTNET_ENVIRONMENT=Development

# Переходим в папку Web.API
cd src/Web.API/ElectronicsComponentWarehouse.Web.API

# Применяем миграции
echo "Применяем миграции к базе данных..."
dotnet ef database update --verbose

# Проверяем результат
if [ $? -eq 0 ]; then
    echo "Миграции успешно применены!"
    
    # Проверяем существование таблиц
    echo "Проверяем существование таблиц..."
    sqlcmd -S "(localdb)\mssqllocaldb" -d "ElectronicsComponentWarehouseDB_Dev" -Q "SELECT name FROM sys.tables ORDER BY name;" | grep -E "(Categories|Components|Users|__EFMigrationsHistory)"
    
    echo "База данных готова к использованию."
else
    echo "Ошибка при применении миграций!"
    exit 1
fi
