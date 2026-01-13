#!/bin/bash

# Скрипт для тестирования аутентификации

API_URL="http://localhost:5000"

echo "Тестирование аутентификации..."

# Вход администратора
echo "1. Вход администратора..."
ADMIN_LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }')

echo "Ответ входа администратора:"
echo "$ADMIN_LOGIN_RESPONSE" | python -m json.tool 2>/dev/null || echo "$ADMIN_LOGIN_RESPONSE"

# Извлекаем токен администратора
ADMIN_TOKEN=$(echo "$ADMIN_LOGIN_RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo ""
echo "Токен администратора: ${ADMIN_TOKEN:0:50}..."

# Вход пользователя
echo ""
echo "2. Вход обычного пользователя..."
USER_LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user",
    "password": "User123!"
  }')

echo "Ответ входа пользователя:"
echo "$USER_LOGIN_RESPONSE" | python -m json.tool 2>/dev/null || echo "$USER_LOGIN_RESPONSE"

# Извлекаем токен пользователя
USER_TOKEN=$(echo "$USER_LOGIN_RESPONSE" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
echo ""
echo "Токен пользователя: ${USER_TOKEN:0:50}..."

# Проверка валидности токена администратора
echo ""
echo "3. Проверка валидности токена администратора..."
VALIDATE_RESPONSE=$(curl -s -X GET "$API_URL/api/Auth/validate" \
  -H "Authorization: Bearer $ADMIN_TOKEN")

echo "Ответ проверки токена:"
echo "$VALIDATE_RESPONSE" | python -m json.tool 2>/dev/null || echo "$VALIDATE_RESPONSE"

echo ""
echo "Тестирование аутентификации завершено!"
