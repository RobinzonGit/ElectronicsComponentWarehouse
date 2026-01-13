#!/bin/bash

# Скрипт для удаления базы данных (осторожно!)

echo "ВНИМАНИЕ: Этот скрипт удалит базу данных!"
read -p "Вы уверены? (y/n): " -n 1 -r
echo

if [[ $REPLY =~ ^[Yy]$ ]]; then
    echo "Удаление базы данных..."
    
    # Переходим в папку Web.API
    cd src/Web.API/ElectronicsComponentWarehouse.Web.API
    
    # Удаляем базу данных
    dotnet ef database drop --force --verbose
    
    if [ $? -eq 0 ]; then
        echo "База данных успешно удалена!"
    else
        echo "Ошибка при удалении базы данных!"
    fi
else
    echo "Операция отменена."
fi
