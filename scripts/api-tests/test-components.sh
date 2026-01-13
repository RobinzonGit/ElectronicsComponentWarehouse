#!/bin/bash

# Скрипт для тестирования компонентов

API_URL="http://localhost:5000"

echo "Тестирование работы с компонентами..."

# Получаем токен администратора
echo "1. Аутентификация администратора..."
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }')

TOKEN=$(echo "$LOGIN_RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo "Токен получен: ${TOKEN:0:50}..."

# Получение всех компонентов
echo ""
echo "2. Получение всех компонентов..."
COMPONENTS_RESPONSE=$(curl -s -X GET "$API_URL/api/Components" \
  -H "Authorization: Bearer $TOKEN")

echo "Количество компонентов в ответе:"
echo "$COMPONENTS_RESPONSE" | grep -o '"id"' | wc -l

# Создание нового компонента
echo ""
echo "3. Создание нового компонента..."
CREATE_RESPONSE=$(curl -s -X POST "$API_URL/api/Components" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Arduino Mega 2560",
    "description": "Микроконтроллерная плата с большим количеством пинов",
    "stockQuantity": 15,
    "storageCellNumber": "A-01-03",
    "manufacturer": "Arduino",
    "modelNumber": "A000067",
    "datasheetLink": "https://store.arduino.cc/products/arduino-mega-2560-rev3",
    "minimumStockLevel": 5,
    "unitPrice": 35.50,
    "categoryId": 1
  }')

echo "Ответ создания компонента:"
echo "$CREATE_RESPONSE" | python -m json.tool 2>/dev/null || echo "$CREATE_RESPONSE"

# Извлекаем ID созданного компонента
NEW_ID=$(echo "$CREATE_RESPONSE" | grep -o '"id":[0-9]*' | cut -d':' -f2)
echo "Создан компонент с ID: $NEW_ID"

# Получение созданного компонента
echo ""
echo "4. Получение созданного компонента..."
GET_RESPONSE=$(curl -s -X GET "$API_URL/api/Components/$NEW_ID" \
  -H "Authorization: Bearer $TOKEN")

echo "Ответ получения компонента:"
echo "$GET_RESPONSE" | python -m json.tool 2>/dev/null || echo "$GET_RESPONSE"

# Поиск компонентов
echo ""
echo "5. Поиск компонентов по ключевому слову..."
SEARCH_RESPONSE=$(curl -s -X GET "$API_URL/api/Components/search?searchTerm=Arduino" \
  -H "Authorization: Bearer $TOKEN")

echo "Найдено компонентов:"
echo "$SEARCH_RESPONSE" | grep -o '"id"' | wc -l

echo ""
echo "Тестирование компонентов завершено!"
