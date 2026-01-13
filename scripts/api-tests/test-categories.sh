#!/bin/bash

# Скрипт для тестирования категорий

API_URL="http://localhost:5000"

echo "Тестирование работы с категориями..."

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

# Получение всех категорий
echo ""
echo "2. Получение всех категорий..."
CATEGORIES_RESPONSE=$(curl -s -X GET "$API_URL/api/Categories" \
  -H "Authorization: Bearer $TOKEN")

echo "Количество категорий в ответе:"
echo "$CATEGORIES_RESPONSE" | grep -o '"id"' | wc -l

# Получение иерархии категорий
echo ""
echo "3. Получение иерархии категорий..."
HIERARCHY_RESPONSE=$(curl -s -X GET "$API_URL/api/Categories/hierarchy" \
  -H "Authorization: Bearer $TOKEN")

echo "Иерархия категорий получена. Уровней категорий:"
echo "$HIERARCHY_RESPONSE" | grep -o '"childCategories"' | wc -l

# Создание новой категории
echo ""
echo "4. Создание новой категории..."
CREATE_RESPONSE=$(curl -s -X POST "$API_URL/api/Categories" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Inductors",
    "description": "Катушки индуктивности и дроссели",
    "parentCategoryId": null
  }')

echo "Ответ создания категории:"
echo "$CREATE_RESPONSE" | python -m json.tool 2>/dev/null || echo "$CREATE_RESPONSE"

# Получение корневых категорий
echo ""
echo "5. Получение корневых категорий..."
ROOTS_RESPONSE=$(curl -s -X GET "$API_URL/api/Categories/roots" \
  -H "Authorization: Bearer $TOKEN")

echo "Корневые категории получены. Количество:"
echo "$ROOTS_RESPONSE" | grep -o '"id"' | wc -l

echo ""
echo "Тестирование категорий завершено!"
