namespace RestaurantApp.Models;

public class Dish
{
    // REVIEW: было `public decimal _price;`. Поле — «внутренность» класса, поэтому private.
    //         Снаружи цену читают через свойство Price. А то, что у Price закрытый set —
    //         отличная идея: цену блюда уже не поменять случайно из другого места программы.
    private decimal _price;

    public string Name { get; }
    public string Category { get; }

    public decimal Price
    {
        get => _price;
        private set
        {
            // REVIEW: было `value > 0`. Из-за этого цена 0 не записывалась (бесплатная вода работала
            //         «случайно», только потому что 0 — значение по умолчанию). Правило звучит как
            //         «не отрицательная», значит условие `>= 0`.
            if (value >= 0)
            {
                _price = value;
            }
        }
    }

    // REVIEW: было CookingTime. Единицы измерения в имени — маленькая привычка, которая экономит часы:
    //         сразу видно, что это минуты, а не секунды.
    public int CookingTimeMinutes { get; }

    // REVIEW: опечатка. Было IsVegeterian -> стало IsVegetarian.
    public bool IsVegetarian { get; }

    public Dish(string name, string category, decimal price, int cookingTimeMinutes, bool isVegetarian)
    {
        Name = name;
        Category = category;
        Price = price;
        CookingTimeMinutes = cookingTimeMinutes;
        IsVegetarian = isVegetarian;
    }

    // REVIEW: новое. Благодаря ToString блюдо можно печатать целиком: Console.WriteLine(dish).
    //         Мы используем это при печати меню — не нужно собирать строку руками в каждом месте.
    public override string ToString()
    {
        string vegetarianMark = IsVegetarian ? " [V]" : "";
        return $"{Name} ({Category}) — {Price}₴, {CookingTimeMinutes} min{vegetarianMark}";
    }
}
