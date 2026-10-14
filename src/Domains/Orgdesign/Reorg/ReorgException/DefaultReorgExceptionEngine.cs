namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the default reorg exception calculation.</summary>
public interface IDefaultReorgExceptionModifier
{
    string Describe();
    double Adjust(DefaultReorgException item, double valueValue);
}

public sealed record CountryDefaultReorgExceptionAdjuster(Dictionary<string, double> Factors) : IDefaultReorgExceptionModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(DefaultReorgException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultReorgExceptionAdjuster(double Minimum) : IDefaultReorgExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultReorgException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredDefaultReorgExceptionAdjuster(List<double> Bounds, List<double> Rates) : IDefaultReorgExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DefaultReorgException item, double valueValue)
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

public sealed record RoundingDefaultReorgExceptionAdjuster(double Step) : IDefaultReorgExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DefaultReorgException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DefaultReorgExceptionMaximumOffsetAdjuster(double Weight) : IDefaultReorgExceptionModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(DefaultReorgException item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentDefaultReorgExceptionAdjuster(double Percent) : IDefaultReorgExceptionModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DefaultReorgException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default reorg exception steps in order and keeps a trace of each.</summary>
public sealed class DefaultReorgExceptionEngine(List<IDefaultReorgExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultReorgExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultReorgExceptionEngine DefaultDefaultReorgExceptionEngine()
    {
        return new DefaultReorgExceptionEngine(
            new List<IDefaultReorgExceptionModifier>
            {
                new CountryDefaultReorgExceptionAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.81, ["west"] = 1.19, ["south"] = 1.22 }),
                new MinimumDefaultReorgExceptionAdjuster(29.2),
                new TieredDefaultReorgExceptionAdjuster(
                    new List<double> { 3434.0, 6068.0, 7297.0, 8534.0 },
                    new List<double> { 0.99, 0.95, 1.19, 1.2 }),
                new RoundingDefaultReorgExceptionAdjuster(0.5),
                new DefaultReorgExceptionMaximumOffsetAdjuster(1.37),
                new PercentDefaultReorgExceptionAdjuster(-3.0),
            },
            new List<string>(),
            0.4,
            79394.3);
    }

    public double Compute(DefaultReorgException item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultReorgException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
