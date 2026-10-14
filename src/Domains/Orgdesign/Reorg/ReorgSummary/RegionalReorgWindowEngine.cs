namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional reorg window calculation.</summary>
public interface IRegionalReorgWindowFactor
{
    string Describe();
    double Adjust(RegionalReorgWindow item, double valueValue);
}

public sealed record CategoryRegionalReorgWindowAdjuster(Dictionary<string, double> Factors) : IRegionalReorgWindowFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RegionalReorgWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalReorgWindowAdjuster(double Percent) : IRegionalReorgWindowFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalReorgWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalReorgWindowAdjuster(List<double> Bounds, List<double> Rates) : IRegionalReorgWindowFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalReorgWindow item, double valueValue)
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

/// <summary>Runs the regional reorg window steps in order and keeps a trace of each.</summary>
public sealed class RegionalReorgWindowEngine(List<IRegionalReorgWindowFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalReorgWindowFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalReorgWindowEngine DefaultRegionalReorgWindowEngine()
    {
        return new RegionalReorgWindowEngine(
            new List<IRegionalReorgWindowFactor>
            {
                new CategoryRegionalReorgWindowAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.39, ["online"] = 1.31 }),
                new PercentRegionalReorgWindowAdjuster(19.8),
                new TieredRegionalReorgWindowAdjuster(
                    new List<double> { 1790.0, 8405.0, 9399.0 },
                    new List<double> { 1.0, 0.93, 0.87 }),
            },
            new List<string>(),
            2.2,
            43572.1);
    }

    public double Compute(RegionalReorgWindow item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalReorgWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
