namespace RestaurantApp.Models;

public class Order
{
    // REVIEW: статусы — «договорённые слова». Если писать "New" руками в десяти местах, одна опечатка
    //         ("new" вместо "New") сломает программу молча. Константа — одно место правды.
    //         (Позже такие наборы заменяют на enum — это следующий шаг, сейчас не обязательно.)
    private const string StatusNew = "New";
    private const string StatusCooking = "Cooking";
    private const string StatusReady = "Ready";
    private const string StatusPaid = "Paid";

    // REVIEW: было `public List<Dish> _dishes;` — публичное поле, которое никто не создавал (null).
    //         Компилятор предупреждал (CS8618), а первый же Add упал бы с NullReferenceException.
    //         Теперь: private + список создаётся сразу. Снаружи со списком работают только через методы Order.
    private readonly List<Dish> _dishes = new List<Dish>();

    public int Number { get; }
    public int TableNumber { get; }

    // REVIEW: было — статус приходил в конструктор (можно было создать заказ сразу «paid»).
    //         Теперь заказ всегда рождается «New», а менять статус можно только методом NextStatus().
    public string Status { get; private set; } = StatusNew;

    public bool IsNew => Status == StatusNew;
    public bool IsPaid => Status == StatusPaid;

    // REVIEW: было — отдельное свойство, которое приходило в конструктор и могло разойтись
    //         с реальным списком. Теперь оно считается из списка само и разойтись не может.
    public int DishesCount => _dishes.Count;

    // REVIEW: конструктор стал проще: номер и стол. Статус и блюда у нового заказа всегда одинаковые.
    public Order(int number, int tableNumber)
    {
        Number = number;
        TableNumber = tableNumber;
    }

    // REVIEW: раньше в классе не было ни одного метода — вся логика жила в Program.cs (и копировалась 8 раз).
    //         Теперь «что умеет заказ» написано один раз здесь.

    // Добавляет блюдо quantity раз. Возвращает false, если добавить нельзя.
    public bool AddDish(Dish dish, int quantity = 1)
    {
        if (dish == null || quantity <= 0 || !IsNew)
        {
            return false;
        }

        for (int i = 0; i < quantity; i++)
        {
            _dishes.Add(dish);
        }

        return true;
    }

    // Убирает ВСЕ порции блюда с таким названием. Возвращает true, если что-то убрали.
    public bool RemoveDish(string dishName)
    {
        if (!IsNew)
        {
            return false;
        }

        int removedCount = _dishes.RemoveAll(d => d.Name == dishName);
        return removedCount > 0;
    }

    // Сумма заказа. discountPercent — скидка в процентах; значение вне 0..100 считаем «скидки нет».
    public decimal GetTotalPrice(decimal discountPercent = 0)
    {
        decimal total = _dishes.Sum(d => d.Price);

        if (discountPercent < 0 || discountPercent > 100)
        {
            return total;
        }

        return total * (100 - discountPercent) / 100;
    }

    // Блюда готовятся одновременно, поэтому время заказа = самое долгое блюдо.
    public int GetEstimatedCookingTime()
    {
        // REVIEW: было `order1.Max(...)` без проверки — на пустом заказе программа падала
        //         с InvalidOperationException ("Sequence contains no elements").
        if (_dishes.Count == 0)
        {
            return 0;
        }

        return _dishes.Max(d => d.CookingTimeMinutes);
    }

    // REVIEW: возвращаем КОПИЮ списка. Если бы вернули сам _dishes, любой код снаружи смог бы
    //         сделать .Clear() и обойти все наши проверки.
    public List<Dish> GetDishes()
    {
        return _dishes.ToList();
    }

    // New -> Cooking -> Ready -> Paid. Пустой заказ дальше «New» не пускаем.
    // REVIEW: было — статус менялся прямо в Program.cs (причём в SwitchingOrderStatus он сначала
    //         сбрасывался в "new", а потом ещё и печатался "wrong number" после успеха).
    public bool NextStatus()
    {
        if (_dishes.Count == 0)
        {
            return false;
        }

        switch (Status)
        {
            case StatusNew:
                Status = StatusCooking;
                return true;
            case StatusCooking:
                Status = StatusReady;
                return true;
            case StatusReady:
                Status = StatusPaid;
                return true;
            default:
                return false; // Paid — дальше некуда
        }
    }

    // REVIEW: этот чек раньше был скопирован 8 раз (в ShowingOrderReceipt) и ещё 8 раз (в ShowingAllOrders).
    //         Формат оставили твой — он удачный: группировка GroupBy и выравнивание колонок.
    public void PrintReceipt()
    {
        Console.WriteLine("=============== RECEIPT ===============");
        Console.WriteLine($"Order №{Number} | Table {TableNumber} | Status: {Status}");
        Console.WriteLine("---------------------------------------");

        foreach (var group in _dishes.GroupBy(d => d.Name))
        {
            decimal sum = group.Sum(d => d.Price);
            Console.WriteLine($"{group.Key,-35}   \\   x{group.Count(),-4}   \\   {sum,10}₴");
        }

        Console.WriteLine("---------------------------------------");
        Console.WriteLine($"Dishes count: {DishesCount}");
        Console.WriteLine($"Cooking time: {GetEstimatedCookingTime()} min");
        Console.WriteLine($"TOTAL: {GetTotalPrice()}₴");
    }
}
