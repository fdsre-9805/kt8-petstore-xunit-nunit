# КТ №8. Анализ результатов тестирования

Григорьев Лаврентий Андреевич, группа 3П3.24.NET.

## 1. Задание

Сравнить тестирование Petstore с помощью xUnit и NUnit. В каждом проекте — 38 проверок. Составить таблицы количественного и качественного анализа.

## 2. Средства выполнения

C#, .NET 10, xUnit 2.9.3, NUnit 4.3.2. API: https://petstore.swagger.io/v2.

## 3. Клиент API

Класс PetstoreApi отправляет HTTP-запросы и возвращает статус и JSON ответа. Один и тот же клиент используется в обоих проектах.

[PetstoreApi.cs](xunit-tests/PetstoreApi.cs)

## 4. Подготовка данных

Каждый тест получает отдельного клиента и свои записи. После проверки записи удаляются. В xUnit подготовка и очистка оформлены через IAsyncLifetime, в NUnit — через SetUp и TearDown.

[Фикстура xUnit](xunit-tests/ApiTests.cs) · [Фикстура NUnit](nunit-tests/ApiTests.cs)

## 5. Тесты

Для pet, store и user проверяются статус ответа и поля JSON. Параметр negative выбирает позитивный или негативный сценарий. В xUnit используются Theory и InlineData, в NUnit — TestCase.

| Сущность | xUnit | NUnit |
|---|---|---|
| pet | [PetTests.cs](xunit-tests/PetTests.cs) | [PetTests.cs](nunit-tests/PetTests.cs) |
| store | [StoreTests.cs](xunit-tests/StoreTests.cs) | [StoreTests.cs](nunit-tests/StoreTests.cs) |
| user | [UserTests.cs](xunit-tests/UserTests.cs) | [UserTests.cs](nunit-tests/UserTests.cs) |

## 6. Результаты

![xUnit](screenshots/xunit.png)

![NUnit](screenshots/nunit.png)
