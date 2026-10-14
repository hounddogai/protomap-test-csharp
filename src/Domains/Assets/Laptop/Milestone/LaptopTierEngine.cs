namespace Acme.Workforce.Assets.Laptop.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the laptop tier calculation.</summary>
public interface ILaptopTierModifier
{
    string Describe();
    double Adjust(LaptopTier item, double valueValue);
}

public sealed record RoundingLaptopTierAdjuster(double Step) : ILaptopTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LaptopTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionLaptopTierAdjuster(Dictionary<string, double> Factors) : ILaptopTierModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LaptopTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LaptopTierRevisionOffsetAdjuster(double Weight) : ILaptopTierModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(LaptopTier item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record PercentLaptopTierAdjuster(double Percent) : ILaptopTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LaptopTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLaptopTierAdjuster(double Cap) : ILaptopTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LaptopTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumLaptopTierAdjuster(double Minimum) : ILaptopTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LaptopTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the laptop tier steps in order and keeps a trace of each.</summary>
public sealed class LaptopTierEngine(List<ILaptopTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILaptopTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LaptopTierEngine DefaultLaptopTierEngine()
    {
        return new LaptopTierEngine(
            new List<ILaptopTierModifier>
            {
                new RoundingLaptopTierAdjuster(0.5),
                new RegionLaptopTierAdjuster(new Dictionary<string, double> { ["online"] = 0.95 }),
                new LaptopTierRevisionOffsetAdjuster(1.02),
                new PercentLaptopTierAdjuster(-24.2),
                new CapLaptopTierAdjuster(46446.0),
                new MinimumLaptopTierAdjuster(35.2),
            },
            new List<string>(),
            3.3,
            2607.2);
    }

    public double Price(LaptopTier item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LaptopTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
