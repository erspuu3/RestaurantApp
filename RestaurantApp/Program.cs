using RestaurantApp.Models;

// REVIEW (общее): было ~1100 строк, ~25 отдельных переменных и восемь копий одного и того же кода
//         (order1Case … order8Case, finalOrder1 … finalOrder8 и т.д.). Класс Order лежал рядом, но программа его не
//         использовала. Стало в несколько раз короче: один список заказов и каждая функция написана один раз.
//         Правило на запомнить: если копируешь код и меняешь только цифру — нужен список или объект.

// ===== Data =====
// REVIEW: было — 14 переменных (borsch, mushroomSoup, ...) и меню, напечатанное руками (дважды подряд),
//         плюс 8 копий switch на 14 пунктов. Теперь одно место правды — список.
//         Номер блюда в меню = индекс + 1. Новое блюдо = одна строка здесь, больше ничего менять не надо.
List<Dish> menu = new List<Dish>
{
    new Dish("Borsch", "Soup", 150m, 25, false),
    new Dish("Mushroom soup", "Soup", 150m, 25, true),
    new Dish("Greek salad", "Salad", 200m, 15, true),
    new Dish("Tuna salad", "Salad", 175m, 15, false),
    new Dish("Caesar salad", "Salad", 225m, 15, false),   // REVIEW: Caesar (было Cesar)
    new Dish("Mashed potatoes with Kiev cutlet", "Main", 300m, 25, false),
    new Dish("Rice with chicken teriyaki", "Main", 320m, 25, false),
    new Dish("Garlic yeast buns", "Side", 60m, 10, true),
    new Dish("White bread", "Side", 40m, 10, true),
    new Dish("Black bread", "Side", 40m, 10, true),
    new Dish("Kompot", "Drink", 60m, 0, true),
    new Dish("Coke zero", "Drink", 60m, 0, true),
    new Dish("Sparkling water", "Drink", 0m, 0, true),
    new Dish("Still water", "Drink", 0m, 0, true)         // REVIEW: было stillgWater — опечатка в имени
};

// REVIEW: было order1..order8, order1Status..order8Status, tableChoice1..tableChoice8 (24 переменные).
//         Теперь — один список объектов Order: каждый заказ сам хранит свой стол, статус и блюда.
List<Order> orders = new List<Order>();

// ===== Main loop =====
// REVIEW: было `int option = 8; while (option != 0)` — число 8 «из головы», чтобы цикл начался.
//         Флаг isRunning читается как обычная фраза: «пока работает». Заодно помним: `break` внутри
//         switch выходит только из switch, а не из цикла, поэтому цикл останавливаем флагом.
bool isRunning = true;

while (isRunning)
{
    ShowMainMenu();
    int option = ReadInt("Your choice: ", 0, 7);

    switch (option)
    {
        case 1: ShowMenu(); break;
        case 2: CreateNewOrder(); break;
        case 3: AddDishToOrder(); break;
        case 4: RemoveDishFromOrder(); break;
        case 5: ShowOrderReceipt(); break;
        case 6: ChangeOrderStatus(); break;
        case 7: ShowAllOrders(); break;
        case 0: isRunning = false; break;
    }
}

// ===== Main menu =====
void ShowMainMenu()
{
    Console.WriteLine();
    Console.WriteLine("===RESTAURANT===");
    Console.WriteLine("1.Show menu");
    Console.WriteLine("2.Create new order");
    Console.WriteLine("3.Add dish to the order");
    Console.WriteLine("4.Remove dish from the order");
    Console.WriteLine("5.Show order receipt");
    Console.WriteLine("6.Switch order status");
    Console.WriteLine("7.Show all orders");
    Console.WriteLine("0.Exit");
}

// ===== Input helper =====
// REVIEW: было — около 20 одинаковых `Convert.ToInt32(Console.ReadLine())` без защиты: ввод букв ронял программу
//         (FormatException), а проверки диапазона были размазаны по коду. Теперь одна функция:
//         спрашивает, пока не получит число из нужного диапазона. try/catch — как раз для такого случая.
int ReadInt(string prompt, int min, int max)
{
    while (true)
    {
        Console.Write(prompt);

        try
        {
            int number = Convert.ToInt32(Console.ReadLine());

            if (number >= min && number <= max)
            {
                return number;
            }

            Console.WriteLine($"Wrong number. Enter a number from {min} to {max}");
        }
        catch (FormatException)
        {
            Console.WriteLine("That's not a number. Try again");
        }
        catch (OverflowException)
        {
            Console.WriteLine("The number is too big. Try again");
        }
    }
}

// ===== 1. Show menu =====
// REVIEW: было — меню печаталось вручную (в Showmenu дважды подряд) и без цен. Теперь — цикл по списку menu.
//         Каждая строка печатается через Dish.ToString().
void ShowMenu()
{
    Console.WriteLine("<<<MENU>>>");

    for (int i = 0; i < menu.Count; i++)
    {
        Console.WriteLine($"{i + 1,2}. {menu[i]}");
    }
}

// ===== 2. Create new order =====
// REVIEW: было — выбор «слота» order1..order8, потом стол, затем цепочка из 8 сравнений tableChoice1..8
//         и switch на 8 веток; «0.Exit» на самом деле не выходил, а стол можно было ввести любой.
//         Теперь: просим стол (1-8), проверяем одним запросом LINQ, создаём объект Order.
void CreateNewOrder()
{
    int table = ReadInt("Choose the table# (1-8, 0 - cancel): ", 0, 8);

    if (table == 0)
    {
        return;
    }

    // стол занят, если за ним есть заказ, который ещё не оплачен
    bool tableIsBusy = orders.Any(o => o.TableNumber == table && !o.IsPaid);

    if (tableIsBusy)
    {
        Console.WriteLine("Table already picked");
        return;
    }

    Order order = new Order(orders.Count + 1, table);
    orders.Add(order);
    Console.WriteLine($"Order №{order.Number} for table {table} created");

    AddDishesToOrder(order);
}

// Спрашивает позиции меню и количество, пока пользователь не введёт 0. Потом печатает чек.
// REVIEW: вместо 8 функций order1Case … order8Case (по 14 одинаковых case в каждой) — одна функция.
//         Нужное блюдо берём из списка по индексу: menu[position - 1].
void AddDishesToOrder(Order order)
{
    ShowMenu();

    while (true)
    {
        int position = ReadInt("Choose position to add (0 - done): ", 0, menu.Count);

        if (position == 0)
        {
            break;
        }

        int quantity = ReadInt("How many portions (1-20): ", 1, 20);

        bool added = order.AddDish(menu[position - 1], quantity);
        Console.WriteLine(added ? "Added" : "Can't add: the order is not 'New' anymore");
    }

    order.PrintReceipt();
}

// ===== Helper: choose an existing order =====
// REVIEW: было — в каждой функции свой список «Order 1 … Order 8» напечатан руками, причём даже
//         для заказов, которых не существует. Теперь показываем только реальные заказы.
//         Возвращает номер заказа или 0, если заказов нет / пользователь отменил.
//         Номер заказа = индекс в списке + 1 (заказы не удаляем), поэтому дальше берём orders[number - 1].
int ChooseOrderNumber(string question)
{
    if (orders.Count == 0)
    {
        Console.WriteLine("There are no orders yet");
        return 0;
    }

    foreach (Order order in orders)
    {
        Console.WriteLine($"Order №{order.Number} | Table {order.TableNumber} | {order.Status} | {order.DishesCount} dishes");
    }

    return ReadInt($"{question} (0 - cancel): ", 0, orders.Count);
}

// ===== 3. Add dish to the order =====
// REVIEW: было — спрашивали номер заказа, а потом он нигде не использовался (вызывался Showmenu,
//         который вообще создавал новый заказ). Теперь блюда реально попадают в выбранный заказ.
void AddDishToOrder()
{
    int number = ChooseOrderNumber("Add dish to order#");

    if (number == 0)
    {
        return;
    }

    Order order = orders[number - 1];

    if (!order.IsNew)
    {
        Console.WriteLine("This order is already in progress, it can't be changed");
        return;
    }

    AddDishesToOrder(order);
}

// ===== 4. Remove dish from the order =====
// REVIEW: было (RemoingDish...) — работало только для заказа №1 (один `case 1`), проверка «пустой»
//         делалась всегда для order1, а неверный номер блюда ронял программу (RemoveAt).
//         Теперь — для любого заказа; показываем блюда сгруппированно и просим выбрать из списка.
void RemoveDishFromOrder()
{
    int number = ChooseOrderNumber("Remove dish from order#");

    if (number == 0)
    {
        return;
    }

    Order order = orders[number - 1];

    if (!order.IsNew)
    {
        Console.WriteLine("This order is already in progress, it can't be changed");
        return;
    }

    if (order.DishesCount == 0)
    {
        Console.WriteLine("Order is empty");
        return;
    }

    var groups = order.GetDishes().GroupBy(d => d.Name).ToList();

    for (int i = 0; i < groups.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {groups[i].Key} x{groups[i].Count()}");
    }

    int position = ReadInt("Which dish to remove (0 - cancel): ", 0, groups.Count);

    if (position == 0)
    {
        return;
    }

    order.RemoveDish(groups[position - 1].Key);
    Console.WriteLine("All portions of this dish were removed");
}

// ===== 5. Show order receipt =====
// REVIEW: было — switch на 8 веток, в каждой скопирован весь чек (~15 строк), и падение на пустом заказе.
//         Теперь — три строки: выбрали заказ и попросили его самого напечатать чек.
void ShowOrderReceipt()
{
    int number = ChooseOrderNumber("Show receipt for order#");

    if (number == 0)
    {
        return;
    }

    orders[number - 1].PrintReceipt();
}

// ===== 6. Switch order status =====
// REVIEW: было — глобальная переменная switchingOrderStatus, которая запоминала «последний заказ»:
//         если нажать «>» раньше, чем «+», менялся статус ЧУЖОГО заказа (мы это проверили запуском).
//         Статус сначала сбрасывался в "new", а после успеха печаталось "wrong number", и был только
//         путь new -> paid. Теперь заказ выбирается явно и сам знает свой следующий статус.
void ChangeOrderStatus()
{
    int number = ChooseOrderNumber("Switch status of order#");

    if (number == 0)
    {
        return;
    }

    Order order = orders[number - 1];

    if (order.NextStatus())
    {
        Console.WriteLine($"Order №{order.Number} is now: {order.Status}");
    }
    else
    {
        Console.WriteLine("Can't switch the status (the order is empty or already paid)");
    }
}

// ===== 7. Show all orders =====
// REVIEW: ShowingAllOrders была написана (≈100 строк копипасты), но нигде не вызывалась.
//         Теперь это цикл из трёх строк, подключённый к меню.
void ShowAllOrders()
{
    if (orders.Count == 0)
    {
        Console.WriteLine("There are no orders yet");
        return;
    }

    foreach (Order order in orders)
    {
        order.PrintReceipt();
    }
}
