# TaskApi

## Описание

TaskApi — учебное приложение на ASP.NET Core Web API для работы со списком задач. Проект разрабатывается в рамках пересдачи по дисциплине DevOps. Автоматизация проекта выполняется через GitHub Actions: тестирование, проверки безопасности, обновление версии и changelog, публикация образа в Docker Hub и уведомление в Telegram.

Возможности приложения:

- просмотр всех задач;
- получение задачи по ID;
- добавление новой задачи;
- отметка задачи как выполненной.

Данные хранятся в памяти приложения. После перезапуска список задач очищается.

## Требования

- .NET 8 SDK для запуска из Visual Studio;
- Visual Studio с рабочей нагрузкой «ASP.NET и веб-разработка» для разработки и запуска из IDE;
- Git для клонирования репозитория;
- Docker Desktop с Docker Compose для запуска в контейнере.

При запуске через Docker устанавливать .NET SDK на компьютер не требуется: приложение собирается внутри контейнера.

## Запуск

### Через Visual Studio

1. Клонировать репозиторий на компьютер.
2. Открыть решение `DevOpsTask` в Visual Studio.
3. При необходимости назначить `TaskApi` запускаемым проектом.
4. Выбрать профиль запуска `http`.
5. Запустить приложение сочетанием `Ctrl + F5`.
6. Найти адрес приложения в строке `Now listening on` окна вывода.

Для получения списка задач открыть в браузере:

```
http://localhost:<порт>/api/tasks
```

Вместо `<порт>` указать порт запущенного приложения.

### Через Docker Compose

1. Запустить Docker Desktop.
2. Открыть терминал в корне репозитория, где находится `compose.yaml`.
3. Собрать образ и запустить контейнер:

```powershell
docker compose up -d --build
```

4. Открыть в браузере:

```text
http://localhost:8080/api/tasks
```

Проверить состояние контейнера:

```powershell
docker compose ps
```

Посмотреть логи приложения:

```powershell
docker compose logs task-api
```

Для остановки и удаления контейнера:

```powershell
docker compose down
```

После изменения кода повторно выполнить `docker compose up -d --build`, чтобы пересобрать образ и запустить обновлённое приложение.

### Готовый образ из Docker Hub

Образы публикуются в [fakenosz/devops-task-api](https://hub.docker.com/r/fakenosz/devops-task-api/tags) с тегами вида `vX.Y.Z`.

После успешного задания `publish` откройте вкладку **Tags** в Docker Hub и скопируйте опубликованный тег. В PowerShell выполните:

```powershell
$releaseTag = Read-Host "Введите опубликованный тег, например v0.3.0"
docker pull "fakenosz/devops-task-api:$releaseTag"
docker run -d --name task-api-release -p 127.0.0.1:8080:8080 "fakenosz/devops-task-api:$releaseTag"
```

Если приложение уже запущено через Compose, сначала выполните `docker compose down`, чтобы освободить порт 8080.

Адрес API: `http://localhost:8080/api/tasks`.

Остановка и удаление контейнера:

```powershell
docker stop task-api-release
docker rm task-api-release
```

### Проверка API

Для проверки остальных операций открыть файл `TaskApi.http`, указать адрес приложения в переменной `baseUrl` и нажимать «Отправить запрос» над нужным запросом. Приложение при этом должно работать.

При запуске через Docker в `TaskApi.http` указать:

```http
@baseUrl = http://localhost:8080
```

Доступные запросы:

| Метод | Адрес                      | Назначение                  |
| ----- | -------------------------- | --------------------------- |
| GET   | `/api/tasks`               | Получить список задач       |
| GET   | `/api/tasks/{id}`          | Получить задачу по ID       |
| POST  | `/api/tasks`               | Добавить задачу             |
| PUT   | `/api/tasks/{id}/complete` | Отметить задачу выполненной |

Пример тела запроса для добавления задачи:

```json
{
  "title": "Настроить GitHub Actions"
}
```

## Тестирование

Из корня репозитория:

```powershell
dotnet test DevOpsTask/TaskApi.Tests/TaskApi.Tests.csproj --configuration Release
```

Проект тестов содержит модульные тесты сервиса задач и интеграционные тесты API.

## Автоматизация

При открытии и обновлении PR в `main` запускаются тесты и проверки безопасности. Проверки включают сканирование файлов ClamAV и аудит зависимостей NuGet. По результатам добавляются метки `test-passed` и `sec-passed`.

После слияния PR из ветки `feature/` или `hotfix/` запускается workflow `Release`:

1. Повторно выполняются тесты и проверки безопасности.
2. Программа `ReleaseTools` на C# обновляет файл `version`: для `feature/` повышает minor и сбрасывает patch, для `hotfix/` повышает patch.
3. Обновляется `changelog.md`, создаются коммит и Git-тег. Changelog сохраняется как артефакт запуска, PR получает метки версии и `changelog`.
4. Собирается и публикуется Docker-образ с тегом версии, обновляются описание и README в Docker Hub. В PR добавляется метка `dockerhub`.
5. Программа `TelegramNotifier` на C# отправляет в Telegram уведомление со ссылками и файл changelog.

Повторный запуск для уже обработанного PR не повышает версию повторно. Повторный запуск задания уведомления может отправить сообщение ещё раз.

### Настройки GitHub Actions

В **Settings → Secrets and variables → Actions** необходимо добавить:

| Тип      | Имя                  | Значение                                       |
| -------- | -------------------- | ---------------------------------------------- |
| Secret   | `DOCKERHUB_USERNAME` | Имя пользователя Docker Hub                    |
| Secret   | `DOCKERHUB_TOKEN`    | Токен Docker Hub с правами Read, Write, Delete |
| Secret   | `TELEGRAM_BOT_TOKEN` | Токен Telegram-бота                            |
| Secret   | `TELEGRAM_CHAT_ID`   | ID чата для уведомлений                        |
| Variable | `DOCKERHUB_IMAGE`    | `fakenosz/devops-task-api`                     |

Бот должен быть добавлен в указанный чат и иметь возможность отправлять сообщения и документы. Значения токенов хранятся только в Secrets.

Результаты выполнения доступны во вкладке **Actions**. Публикацию проверяют по тегу образа в Docker Hub, уведомление — по сообщению и вложению в Telegram.
