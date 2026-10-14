namespace Acme.Workforce.Kit.Money;

using System.Globalization;

/// <summary>An amount of money. Arithmetic refuses to mix currencies.</summary>
public sealed record Amount(int Units, string Currency)
{
    public Amount Plus(Amount other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"cannot add {other.Currency} to {Currency}");
        }
        return new Amount(Units + other.Units, Currency);
    }

    public Amount Minus(Amount other)
    {
        if (Currency != other.Currency)
        {
            throw new InvalidOperationException($"cannot subtract {other.Currency} from {Currency}");
        }
        return new Amount(Units - other.Units, Currency);
    }

    public Amount Scaled(double factor)
    {
        var units = (int)Math.Floor((double)Units * factor + 0.5);
        return new Amount(units, Currency);
    }

    public bool IsZero()
    {
        return Units == 0;
    }

    public string Format()
    {
        var major = Units / 100;
        var minor = Math.Abs(Units % 100);
        return $"{Currency} {major}.{minor.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')}";
    }

    public List<Amount> Split(int count)
    {
        var shares = new List<Amount>();
        if (count <= 0)
        {
            return shares;
        }
        var baseValue = Units / count;
        var rest = Units % count;
        for (var i = 0; i < count; i++)
        {
            var units = baseValue;
            if (i < rest)
            {
                units += 1;
            }
            shares.Add(new Amount(units, Currency));
        }
        return shares;
    }
}

/// <summary>Functions of money.</summary>
public static class Money
{
    public static Amount Of(int units, string currency)
    {
        return new Amount(units, currency.Trim().ToUpperInvariant());
    }

    public static Amount Total(List<Amount> amounts, string currency)
    {
        var sum = Of(0, currency);
        foreach (var item in amounts)
        {
            var next = sum.Plus(item);
            sum = next;
        }
        return sum;
    }
}
