# TaskApi

## Описание

Учебное приложение на ASP.NET Core для работы со списком задач: просмотр, добавление и отметка выполнения. Данные хранятся в памяти и очищаются после перезапуска приложения.

## Требования

- Docker Desktop с Docker Compose — для запуска в контейнере.
- .NET 8 SDK — для запуска из исходников и выполнения тестов.

Команды ниже выполняются в PowerShell из корня репозитория. При запуске через Docker .NET SDK на компьютере не требуется.

## Запуск

### Из исходников

```powershell
dotnet run --project DevOpsTask/TaskApi/TaskApi.csproj --launch-profile http
```

Адрес приложения указан в строке `Now listening on`. Список задач доступен по пути `/api/tasks`.

В Visual Studio можно открыть решение `DevOpsTask/DevOpsTask.slnx`, выбрать проект `TaskApi`, профиль `http` и нажать `Ctrl + F5`.

### Через Docker Compose

Запустить Docker Desktop, затем выполнить:

```powershell
docker compose up -d --build
```

API: <http://localhost:8080/api/tasks>.

Остановка:

```powershell
docker compose down
```

### Готовый образ из Docker Hub

Выбрать опубликованный тег в [Docker Hub](https://hub.docker.com/r/fakenosz/devops-task-api/tags). Порт 8080 должен быть свободен; одновременно запускать оба варианта Compose не нужно.

```powershell
$env:DOCKERHUB_IMAGE = "fakenosz/devops-task-api"
$env:RELEASE_TAG = Read-Host "Введите опубликованный тег версии"
$env:COMPOSE_PROJECT_NAME = "devops-task-prod"

docker compose -f compose.prod.yaml pull
docker compose -f compose.prod.yaml up -d
```

API: <http://localhost:8080/api/tasks>.

Остановка в том же окне PowerShell:

```powershell
docker compose -f compose.prod.yaml down
```

Этот вариант использует тот же Compose-проект, что и автоматический деплой. Команда остановки также остановит приложение, запущенное pipeline.

## Проверка

Запросы для ручной проверки находятся в `DevOpsTask/TaskApi/TaskApi.http`. Перед отправкой указать адрес приложения в переменной `baseUrl`.

Запуск тестов:

```powershell
dotnet test DevOpsTask/TaskApi.Tests/TaskApi.Tests.csproj --configuration Release
```

## Автоматизация

Слияние веток `feature/` и `hotfix/` в `main` запускает проверку, выпуск версии, публикацию образа, уведомление Telegram, локальный деплой и создание GitHub Release. Конфигурация находится в `.github/workflows`.

Для локального деплоя должны работать Docker Desktop и Windows runner с меткой `task-api-prod`. Приложение доступно на компьютере runner по адресу <http://localhost:8080/api/tasks>.

## Telegram группа

Ссылка на группу с обновлениями: https://t.me/+4U8z-NuIEoU2OTIy

![](screenshots\2026-10-16%2000-35-14.png)
