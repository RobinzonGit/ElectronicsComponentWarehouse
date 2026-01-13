#!/bin/bash

# Полный тест API

echo "=== ПОЛНЫЙ ТЕСТ API СИСТЕМЫ УПРАВЛЕНИЯ СКЛАДОМ ==="
echo ""

# Запускаем API сервер в фоне
echo "1. Запуск API сервера..."
cd src/Web.API
dotnet run --no-build > /tmp/api-test.log 2>&1 &
API_PID=$!
sleep 15

if ! ps -p $API_PID > /dev/null; then
    echo "❌ Не удалось запустить API сервер"
    cat /tmp/api-test.log
    exit 1
fi

echo "✅ API сервер запущен (PID: $API_PID)"
echo ""

# Тестируем аутентификацию
echo "2. Тестирование аутентификации..."
source ../scripts/api-tests/test-auth.sh
echo ""

# Тестируем категории
echo "3. Тестирование категорий..."
source ../scripts/api-tests/test-categories.sh
echo ""

# Тестируем компоненты
echo "4. Тестирование компонентов..."
source ../scripts/api-tests/test-components.sh
echo ""

# Проверяем статистику
echo "5. Проверка статистики..."
STATS_RESPONSE=$(curl -s -X GET "http://localhost:5000/api/Components/statistics" \
  -H "Authorization: Bearer $TOKEN")

echo "Статистика компонентов:"
echo "$STATS_RESPONSE" | python -m json.tool 2>/dev/null || echo "$STATS_RESPONSE"
echo ""

# Останавливаем сервер
echo "6. Остановка API сервера..."
kill $API_PID
wait $API_PID 2>/dev/null

echo ""
echo "=== ТЕСТИРОВАНИЕ ЗАВЕРШЕНО ==="
echo "Логи сервера сохранены в /tmp/api-test.log"
