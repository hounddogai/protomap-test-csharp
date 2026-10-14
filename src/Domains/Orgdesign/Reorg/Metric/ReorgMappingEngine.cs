namespace Acme.Workforce.Orgdesign.Reorg.Metric;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the reorg mapping calculation.</summary>
public interface IReorgMappingAdjuster
{
    string Describe();
    double Adjust(ReorgMapping item, double valueValue);
}

public sealed record CountryReorgMappingAdjuster(Dictionary<string, double> Factors) : IReorgMappingAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ReorgMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ReorgMappingExpiresAtOffsetAdjuster(double Weight) : IReorgMappingAdjuster
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ReorgMapping item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record MinimumReorgMappingAdjuster(double Minimum) : IReorgMappingAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReorgMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredReorgMappingAdjuster(List<double> Bounds, List<double> Rates) : IReorgMappingAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReorgMapping item, double valueValue)
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

public sealed record CapReorgMappingAdjuster(double Cap) : IReorgMappingAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReorgMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the reorg mapping steps in order and keeps a trace of each.</summary>
public sealed class ReorgMappingEngine(List<IReorgMappingAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReorgMappingAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReorgMappingEngine DefaultReorgMappingEngine()
    {
        return new ReorgMappingEngine(
            new List<IReorgMappingAdjuster>
            {
                new CountryReorgMappingAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.78, ["west"] = 1.05 }),
                new ReorgMappingExpiresAtOffsetAdjuster(0.54),
                new MinimumReorgMappingAdjuster(34.4),
                new TieredReorgMappingAdjuster(
                    new List<double> { 1432.0, 5480.0, 9206.0 },
                    new List<double> { 1.07, 1.14, 0.88 }),
                new CapReorgMappingAdjuster(15009.0),
            },
            new List<string>(),
            4.9,
            72468.1);
    }

    public double Evaluate(ReorgMapping item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ReorgMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
