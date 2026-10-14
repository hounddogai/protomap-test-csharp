namespace Acme.Workforce.Assets.Laptop.Adjustment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped laptop rollup calculation.</summary>
public interface ICappedLaptopRollupModifier
{
    string Describe();
    double Adjust(CappedLaptopRollup item, double valueValue);
}

public sealed record CapCappedLaptopRollupAdjuster(double Cap) : ICappedLaptopRollupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedLaptopRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingCappedLaptopRollupAdjuster(double Step) : ICappedLaptopRollupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedLaptopRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentCappedLaptopRollupAdjuster(double Percent) : ICappedLaptopRollupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedLaptopRollup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedLaptopRollupWeightOffsetAdjuster(double Weight) : ICappedLaptopRollupModifier
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(CappedLaptopRollup item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record MinimumCappedLaptopRollupAdjuster(double Minimum) : ICappedLaptopRollupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedLaptopRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredCappedLaptopRollupAdjuster(List<double> Bounds, List<double> Rates) : ICappedLaptopRollupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedLaptopRollup item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

/// <summary>Runs the capped laptop rollup steps in order and keeps a trace of each.</summary>
public sealed class CappedLaptopRollupEngine(List<ICappedLaptopRollupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedLaptopRollupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedLaptopRollupEngine DefaultCappedLaptopRollupEngine()
    {
        return new CappedLaptopRollupEngine(
            new List<ICappedLaptopRollupModifier>
            {
                new CapCappedLaptopRollupAdjuster(8467.0),
                new RoundingCappedLaptopRollupAdjuster(0.05),
                new PercentCappedLaptopRollupAdjuster(2.9),
                new CappedLaptopRollupWeightOffsetAdjuster(0.64),
                new MinimumCappedLaptopRollupAdjuster(23.4),
                new TieredCappedLaptopRollupAdjuster(
                    new List<double> { 2161.0, 5718.0, 6630.0, 7744.0 },
                    new List<double> { 0.87, 1.06, 0.85, 1.15 }),
            },
            new List<string>(),
            4.8,
            68795.6);
    }

    public double Price(CappedLaptopRollup item)
    {
        var current = item.ProjectedTotal();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CappedLaptopRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
