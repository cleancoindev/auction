# Интеграция TradableAsset и аукциона Expload

В данном гиде содержится подробное пошаговое описание интеграции децентрализованной игры в инфраструктуру Expload - инвентарь и аукцион.

## TradableAsset - стандарт и пример реализации

### Обзор

Для совместимости игры с инвентарем и аукционом Expload, все внутриигровые предметы (далее - ассеты) должны храниться в pravda-программе, соответствующей одному из интерфейсов: [ITradableXGAsset](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/ITradableXGAsset.cs) или [ITradableXPAsset](https://github.com/expload/auction/blob/master/TradableAsset/source/XP/ITradableXPAsset.cs). 
Интерфейс `ITradableXGAsset` предназначен для ассетов, которые могут продаваться только за XGold. Аналогично, интерфейс `ITradableXPAsset` предназначен для ассетов, которые могут продаваться только за XPlatinum. XGold и XP интерфейсы отличаются лишь неймингом методов (к примеру, `GetXGAsset` и `GetXPAsset`), их логика аналогична.  

Рассмотрим эти интерфейсы на [примере реализации XGold-ассета](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/TradableXGAsset.cs).

> Сниппеты из кода образца реализации приведены для понимания внутренней логики ассетов, рассматривать эти сниппеты не обязательно. Если ваша игра не требует особых механизмов передачи или создания ассетов, вы можете использовать образцовую реализацию в вашей игре, и пользоваться ей как API. Ознакомьтесь с структурой ассета и интерфейсами методов ниже и переходите к разделу [Деплой и настройка имплементации](#Деплой-и-настройка-имплементации).

### Структура ассета

Каждый объект ассета имеет следующие поля:

`long Id` (далее - `id`) - блокчейн-id ассета. Не имеет никакого внутриигрового значения. Используется только внутри смарт-контрактов как уникальный идентификатор ассета.

`Bytes Owner` - адрес текущего владельца пользователя.

`Bytes ItemClassId` (далее - `classId`) - внутриигровой id определенного класса ассетов. _Например, два внутриигровых меча "Зов Небес", принадлежащие различным игрокам, и по-разному апгрейднутые владельцами имеют одинаковый `classId`._ Предметы с одинаковым `classId` будут иметь единую вкладку на аукционе.

`Bytes ItemInstanceId` (далее - `instanceId`) - внутриигровой id конкретной копии, инстанции ассета. _Например, два внутриигровых меча "Зов Небес", принадлежащие различным игрокам, и по-разному апгрейднутые владельцами имеют разный `instanceId`._

### Операции с ассетами

#### Создание ассета

`EmitXGAsset(Bytes owner, Bytes classId, Bytes  instanceId) returns long id` - создает ассет с заданными параметрами и передает его адресу `owner`. Возвращает уникальный `id` ассета. Доступно только создателю вызываемого контракта.

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
public long EmitXGAsset(Bytes owner, Bytes classId, Bytes instanceId){
    // Проверяем на то, является ли адрес, вызывающий
    // метод владельцем контракта
    AssertIsGameOwner();

    // _lastXGId - глобальная переменная, в которой хранится
    // последний id, выданный ассету, увеличиваем на 1
    var id = ++_lastXGId;

    // Создаем объект класса Asset
    var asset = new Asset(id, owner, classId, instanceId);

    // Добавляем ассет в mapping _XGAssets - основное хранилище ассетов
    _XGAssets[id] = asset;

    /*
    Кроме основного хранилища ассетов, имеется
    хранилище ассетов для каждого пользователя.
    Таким образом, достать все ассеты, принадлежащие
    Конкретному пользователю становится очень просто.
    */

    // Добавим ассет в хранилище пользователя

    // Получим из мэппинга _XGUsersAssetCount
    // текущее количество ассетов у пользователя
    // В ячейку с этим номером будет размещен новый ассет
    var assetCount = _XGUsersAssetCount.GetOrDefault(owner, 0);

    // Записываем id ассета в хранилище пользователей
    _XGUsersAssetIds[GetUserAssetKey(owner, assetCount)] = id;
    // Увеличиваем количество ассетов пользователя на 1
    _XGUsersAssetCount[owner] = assetCount + 1;
    // Добавляем порядковый номер id ассета в хранилище порядковых номеров
    // (это нужно, чтобы не итерироваться по ассетам пользователя
    // при передаче ассетов)
    _SerialNumbers[id] = assetCount;

    // Вызываем событие
    Log.Event("EmitXG", asset);

    // Возвращаем id созданного ассета
    return id;
}
```

</details>

#### Передача ассета

`TransferXGAsset(long id, bytes to)` - передает ассет с заданным `id` адресу `to`. Доступно только аукциону Expload.

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
public void TransferXGAsset(long id, Bytes to){
    // Проверяем на то, является ли адрес, вызывающий
    // метод владельцем аукционом Expload
    AssertIsAuction();

    // Получаем объект ассета по данному id
    var asset = GetXGAsset(id);
    // Получаем старого владельца ассета
    var oldOwner  = asset.Owner;

    // Проверяем, существует ли этот ассет на самом деле
    // (есть ли у него владелец)
    if(oldOwner == Bytes.VOID_ADDRESS){
        Error.Throw("This asset doesn't exist.");
    }

    // Меняем владельца ассета
    asset.Owner = to;
    // Записываем измененный ассет в общее хранилище
    _XGAssets[id] = asset;

    // Теперь необходимо внести изменения в хранилище ассетов пользователей

    // Удаляем из хранилища старого владельца

    // Получаем количество ассетов старого владельца
    var oldOwnerAssetCount = _XGUsersAssetCount.GetOrDefault(oldOwner, 0);
    // Получаем порядковый номер ассета в хранилище владельца
    var oldOwnerSerialNumber = _SerialNumbers.GetOrDefault(id, 0);
    // Получаем последний ассет в хранилище владельца
    var lastAsset = _XGUsersAssetIds.GetOrDefault(GetUserAssetKey(oldOwner, oldOwnerAssetCount-1), 0);
    // Ставим последний ассет на место ассета, который забираем у владельца
    _XGUsersAssetIds[GetUserAssetKey(oldOwner, oldOwnerSerialNumber)] = lastAsset;
    // Удаляем последний ассет (так как он теперь на месте проданного)
    _XGUsersAssetIds[GetUserAssetKey(oldOwner,oldOwnerAssetCount-1)] = 0;
    // Уменьшаем счетчик ассетов
    _XGUsersAssetCount[oldOwner] = oldOwnerAssetCount - 1;

    // Добавляем в хранилище нового владельца

    // Получаем количество ассетов нового владельца
    var assetCount = _XGUsersAssetCount.GetOrDefault(to, 0);
    // Добавляем ассет в хранилище ассетов пользователей
    _XGUsersAssetIds[GetUserAssetKey(to, assetCount)] = id;
    // Увеличиваем количество ассетов у пользователя
    _XGUsersAssetCount[to] = assetCount + 1;

    // Обновляем порядковый номер ассета в хранилище владельца
    _SerialNumbers[id] = newSerialNumber;

    // Вызываем событие
    Log.Event("TransferXG", asset);
}
```

</details>

### Получение данных

#### Получение данных об ассете по `id`

`GetXGAssetData(long id) returns Asset asset` - возвращает объект ассета, имеющего соответствующий `id`.

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
// Приватный метод для получения объекта ассета из общего хранилища
// (Используется во многих функциях получения данных из контракта)
private Asset GetXGAsset(long id){
    return _XGAssets.GetOrDefault(id, new Asset());
}

public Asset GetXGAssetData(long id){
    // Вызов приватного метода для получения ассета
    return GetXGAsset(id);
}
```

</details>

#### Получение владельца ассета по `id`

`GetXGAssetOwner(long id) returns Bytes owner` - возвращает адрес текущего владельца ассета, имеющего соответствующий `id`

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
public Bytes GetXGAssetOwner(long id){
    // Используем приватный метод описанный ранее
    return GetXGAsset(id).Owner;
}
```

</details>

#### Получение `classId` ассета по `id`

`GetXGAssetClassId(long id) returns Bytes classId` - возвращает `classId` ассета, имеющего соответствующий `id`

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
public Bytes GetXGAssetClassId(long id){
    // Используем приватный метод описанный ранее
    return GetXGAsset(id).ItemClassId;
}
```

</details>

#### Получение количества ассетов у пользователя

`GetUsersXGAssetCount(Bytes address) returns long assetCount` - возвращает количество ассетов, принадлежащих пользователю с адресом `address`

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
public long GetUsersXGAssetCount(Bytes address){\
    // Достаем нужное значение из мэппинга
    return _XGUsersAssetCount.GetOrDefault(address, 0);
}
```

</details>

#### Получение `id` ассета по порядковому номеру в хранилище пользователя

`GetUsersXGAssetId(Bytes address, long number) returns long id` - возвращает `id` ассета, имеющего порядковый номер `number` в хранилище пользователя с адресом `address`

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
// Приватный метод для получения нужного id
// (Также используется в других методах)
private long _getUsersXGAssetId(Bytes address, long number){
    // Порядковый номер не может быть больше,
    // чем количество ассетов у пользователя
    if(number >= _XGUsersAssetCount.GetOrDefault(address, 0)){
        Error.Throw("This asset doesn't exist!");
    }

    // Достаем нужный id из хранилища ассетов пользователя
    var key = GetUserAssetKey(address, number);
    return _XGUsersAssetIds.GetOrDefault(key, 0);
}

public long GetUsersXGAssetId(Bytes address, long number){
    // Вызываем приватный метод и возвращаем id
    return _getUsersXGAssetId(address, number);
}
```

</details>

#### Получение всех ассетов, принадлежащих пользователю

`GetUsersAllXGAssetsData(Bytes address) returns Asset[] inventory` - возвращает массив объектов ассетов, принадлежащих пользователю с адресом `address`.

<details>

<summary>Рассмотрение образца имплементации</summary> 

Рассмотрим реализацию этого метода (комментарии изменены для удобства)

```c#
public Asset[] GetUsersAllXGAssetsData(Bytes address){
    // Получаем количество ассетов у пользователя
    int amount = (int)_XGUsersAssetCount.GetOrDefault(address, 0);
    // Создаем пустой массив
    var result = new Asset[amount];

    // Заполняем его ассетами
    for(int num = 0; num < amount; num++){
        // Получаем id ассета из приватного метода, описанного выше,
        // затем получаем объект ассета по этому id
        result[num] = GetXGAsset(_getUsersXGAssetId(address, num));
    }
    return result;
}

```

</details>

### Деплой и настройка имплементации

Теперь, когда у нас есть понимание устройства ассета и того, какие методы используются в стандарте, перейдем к настройке контракта.  
Настройка контракта будет рассмотрена на [примере реализации XGold-ассета](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/TradableXGAsset.cs).

#### Деплой pravda-программы

```sh
# Склонируем репозиторий с имплементацией
git clone https://github.com/expload/auction

# Перейдем в папку с имплементацией
cd auction/TradableAsset/source/XG

# Сгенерируем кошелек wallet.json - с него будем проводить деплой
pravda gen address -o wallet.json

# Кошелек нужно пополнить перед деплоем контракта:
# https://faucet.dev.expload.com/ui

# Теперь можно сделать деплой программы в test-net:
dotnet publish -c Deploy

# В текущей директории появится program-wallet.json.
# Это 'администраторский' кошелек - с помощью него
# можно выполнять основную конфигурацию контракта.
```

Адрес из `program-wallet.json` необходимо сообщить представителю Expload для внесения его в общую базу игр.

#### Конфигурация контракта

Действия выполняются из директории деплоя (`auction/TradableAsset/source/XG`)

```sh
# Необходимо выставить в контракте текущий адрес аукциона Expload
# (чтобы аукцион имел право передавать внутриигровые предметы)

# Актуальный адрес аукциона вам сообщат представители Expload
# Для этого туториала мы считаем, что адрес аукциона - A,
# Адрес program-wallet.json - B

# Не забудьте пополнить кошелек program-wallet.json
# И изменить адреса на валидные перед выполнением

echo "push xA push \"SetAuction\" push xB push 2 pcall" | pravda compile asm | pravda broadcast run -w program-wallet.json -l 9000000
```

```sh
# Также можно Аукциону указать процент комиссии, который будет
# взиматься с продажи каждого ассета.
# По умолчанию процент комиссии равен нулю.

# Для этого у программы Аукциона вызовем метод SetGameCommission(percent, gameId, isXG)
echo "push {percent} push {gameId} push true push \"SetGameCommission\" push xA push 4 pcall" | pravda compile asm | pravda broadcast run -w program-wallet.json -l 9000000

# Где {percent} - это % комиссии от суммы, которую получит продавец после продажи.
# {gameId} - id (:long) вашей программы, которую назначил вам Аукцион.
# Пример. Если указать 5 процентов, то при стоимости лота в 105 монет -
# продавец получит 100 монет и ваша программа - 5 монет. 
```

Поздравляем! Ваш контракт настроен и готов к работе.

### Разработка собственной имплементации

Expload поддерживает собственную имплементацию стандарта ассетов. Имплементация должна соответствовать одному из интерфейсов - [ITradableXGAsset](https://github.com/expload/auction/blob/master/TradableAsset/source/XG/ITradableXGAsset.cs) или [ITradableXPAsset](https://github.com/expload/auction/blob/master/TradableAsset/source/XP/ITradableXPAsset.cs) (в зависимости от типа ассета).

Для удобства был создан [nuget-пакет](https://www.nuget.org/packages/Expload.Standards.TradableAsset/), включающий в себя оба интерфейса.

## MetaData

Для интеграции необходима не только реализация стандарта ассетов, но и meta-сервер, предоставляющий для UI Expload все необходимые данные об ассетах - название, описание, картинка и так далее.

### Структура мета-данных

Мета данные имеют следующий формат:

```json
{
    "name": <itemName>,
    "description": <itemShortDescription>,
    "pictureUrl": <itemPictureURL>,
    "previewPictureUrl": <itemPreviewPictureURL>,
    "tags": <tagsData>
}
```

`name` - название предмета. _К примеру, "Меч-кладенец"_.  
`description` - короткое описание предмета. _К примеру, "Реликвия древнерусских войнов, канувшая в летах"_.  
`pictureUrl` - ссылка на картинку предмета. Формат `.png`, прозрачный фон, предпочтительно вписывать в квадрат. Максимальный размер - 512px x 512 px.  
`previewPictureUrl` - ссылка на уменьшенную (превью) картинку предмета. Правила для картинки такие же, как и у `pictureUrl`.  
`tags` - словарь с остальными данными, характеристиками предмета. _К примеру:_  

```json
{
    "name": "Меч-кладенец",
    "description": "Реликвия древнерусских войнов, канувшая в летах",
    "pictureUrl" : "some_url.com/kladenec.png",
    "previewPictureUrl" : "some_url.com/kladenec_small.png",
    "tags" :  {
        "rarity": ["legendary"],
        "race": ["orcs", "humans"]
    }
}
```

Данные в `tags` могут быть использованы для фильтрации и поиска товаров в аукционе.

Как было [описано ранее](#Структура-ассета), каждый ассет имеет 2 идентификатора, имеющие внутриигровое значение - `classId` и `instanceId`. Для каждого ассета необходимо предоставлять мета-данные по каждому из этих двух идентификаторов. Мета-данные по `classId` будут показаны в аукционе при выборе предмета для покупки, по `instanceId` - при выборе конкретных лотов на аукционе и в инвентаре.
Грубо говоря, `classId` является более общей информацией, чем `instanceId`, показывающая характеристики конкретной инстанции предмета. Данные, имеющиеся в мете класса, не обязательно дублировать в мете инстанции. Если какое-либо поле присутствует и в мете класса, и в мете инстанции, то будет использоваться поле из меты класса.

### Мета-сервер

У игры должен быть мета сервер, который будет через публичное API отдавать instance- и class-мету.  
Доступен [пример имплементации](https://github.com/krylov-na/TestMetaServer) такого сервера на C#.

### Настройка программы

Когда ваш мета-сервер готов, необходимо добавить информацию о нем в pravda-программу стандарта ассетов. Модифицируйте методы `GetClassIdMeta` и `GetInstanceIdMeta` таким образом, чтобы при передачи в них соответствующих идентификаторов возвращалась строка с ссылкой на мета-данные ([на мета-сервере](#Мета-сервер)) по данному id.

После того, как вы должном образом изменили код, необходимо или [задеплоить программу](#Деплой-и-настройка-имплементации), или, если вы это уже сделали, выполнить следующую команду в папке проекта:

```sh
# Важно! Для собственных имплементаций это будет работать
# только в случае разработки с помощью PravdaProgramTemplate:
# https://bit.ly/2ARW0uR

dotnet publish -c Update
```

## Аукцион

Если вы выполнили все предыдущие шаги, и представитель Expload присвоил вашей игре `GameId`, то вы готовы к работе с аукционом!

Ваша игра будет отображаться в аукционе в приложении Expload. Вы также можете сделать дополнительный интерфейс аукциона внутри своей игры, используя [исходный код аукциона](https://github.com/expload/auction/blob/master/Auction/source/Auction.cs).
