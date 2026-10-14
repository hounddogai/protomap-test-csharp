namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped visitor window calculation.</summary>
public interface ICappedVisitorWindowStep
{
    string Describe();
    double Adjust(CappedVisitorWindow item, double valueValue);
}

public sealed record RegionCappedVisitorWindowAdjuster(Dictionary<string, double> Factors) : ICappedVisitorWindowStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CappedVisitorWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingCappedVisitorWindowAdjuster(double Step) : ICappedVisitorWindowStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedVisitorWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedVisitorWindowAdjuster(List<double> Bounds, List<double> Rates) : ICappedVisitorWindowStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedVisitorWindow item, double valueValue)
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

public sealed record PercentCappedVisitorWindowAdjuster(double Percent) : ICappedVisitorWindowStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedVisitorWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapCappedVisitorWindowAdjuster(double Cap) : ICappedVisitorWindowStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedVisitorWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped visitor window steps in order and keeps a trace of each.</summary>
public sealed class CappedVisitorWindowEngine(List<ICappedVisitorWindowStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedVisitorWindowStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedVisitorWindowEngine DefaultCappedVisitorWindowEngine()
    {
        return new CappedVisitorWindowEngine(
            new List<ICappedVisitorWindowStep>
            {
                new RegionCappedVisitorWindowAdjuster(new Dictionary<string, double> { ["north"] = 1.1 }),
                new RoundingCappedVisitorWindowAdjuster(1.0),
                new TieredCappedVisitorWindowAdjuster(
                    new List<double> { 2832.0, 3736.0, 5444.0, 7828.0 },
                    new List<double> { 0.99, 0.94, 0.92, 1.1 }),
                new PercentCappedVisitorWindowAdjuster(-23.5),
                new CapCappedVisitorWindowAdjuster(31017.0),
            },
            new List<string>(),
            4.8,
            85543.5);
    }

    public double Compute(CappedVisitorWindow item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedVisitorWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
