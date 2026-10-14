namespace Acme.Workforce.Assets.Warranty.Summary;

/// <summary>One step of the warranty summary calculation.</summary>
public interface IWarrantySummaryModifier
{
    string Describe();
    double Adjust(WarrantySummary item, double valueValue);
}

public sealed record CapWarrantySummaryAdjuster(double Cap) : IWarrantySummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WarrantySummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredWarrantySummaryAdjuster(List<double> Bounds, List<double> Rates) : IWarrantySummaryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantySummary item, double valueValue)
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

public sealed record LabelWarrantySummaryAdjuster(Dictionary<string, double> Factors) : IWarrantySummaryModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(WarrantySummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record WarrantySummaryMaximumOffsetAdjuster(double Weight) : IWarrantySummaryModifier
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(WarrantySummary item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentWarrantySummaryAdjuster(double Percent) : IWarrantySummaryModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantySummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the warranty summary steps in order and keeps a trace of each.</summary>
public sealed class WarrantySummaryEngine(List<IWarrantySummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantySummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantySummaryEngine DefaultWarrantySummaryEngine()
    {
        return new WarrantySummaryEngine(
            new List<IWarrantySummaryModifier>
            {
                new CapWarrantySummaryAdjuster(3482.0),
                new TieredWarrantySummaryAdjuster(
                    new List<double> { 963.0, 5989.0 },
                    new List<double> { 1.13, 1.03 }),
                new LabelWarrantySummaryAdjuster(new Dictionary<string, double> { ["north"] = 1.1, ["east"] = 0.91 }),
                new WarrantySummaryMaximumOffsetAdjuster(1.55),
                new PercentWarrantySummaryAdjuster(23.4),
            },
            new List<string>(),
            1.3,
            63020.5);
    }

    public double Compute(WarrantySummary item)
    {
        var current = item.EffectiveValue();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<WarrantySummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
