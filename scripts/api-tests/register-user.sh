#!/bin/bash

# Скрипт для регистрации пользователя через API

API_URL="http://localhost:5000"

echo "Регистрация пользователя через API..."

# Регистрация администратора
echo "1. Регистрация администратора..."
ADMIN_RESPONSE=$(curl -s -X POST "$API_URL/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!",
    "email": "admin@warehouse.local",
    "fullName": "Администратор Системы",
    "role": "Admin"
  }')

echo "Ответ: $ADMIN_RESPONSE"

# Регистрация обычного пользователя
echo ""
echo "2. Регистрация обычного пользователя..."
USER_RESPONSE=$(curl -s -X POST "$API_URL/api/Auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user",
    "password": "User123!",
    "email": "user@warehouse.local",
    "fullName": "Обычный Пользователь",
    "role": "User"
  }')

echo "Ответ: $USER_RESPONSE"

echo ""
echo "Тестирование регистрации завершено!"
