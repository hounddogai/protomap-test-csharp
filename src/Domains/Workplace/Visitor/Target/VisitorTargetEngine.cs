namespace Acme.Workforce.Workplace.Visitor.Target;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor target calculation.</summary>
public interface IVisitorTargetStep
{
    string Describe();
    double Adjust(VisitorTarget item, double valueValue);
}

public sealed record PercentVisitorTargetAdjuster(double Percent) : IVisitorTargetStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredVisitorTargetAdjuster(List<double> Bounds, List<double> Rates) : IVisitorTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorTarget item, double valueValue)
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

public sealed record FlaggedVisitorTargetAdjuster(Registry Flags, string Flag, double Percent) : IVisitorTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(VisitorTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the visitor target steps in order and keeps a trace of each.</summary>
public sealed class VisitorTargetEngine(List<IVisitorTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorTargetEngine DefaultVisitorTargetEngine(Registry flags)
    {
        return new VisitorTargetEngine(
            new List<IVisitorTargetStep>
            {
                new PercentVisitorTargetAdjuster(-9.1),
                new TieredVisitorTargetAdjuster(new List<double> { 1645.0, 7534.0 }, new List<double> { 0.81, 1.01 }),
                new FlaggedVisitorTargetAdjuster(flags, "visitor-target-global", 0.9),
            },
            new List<string>(),
            1.5,
            24387.5);
    }

    public double Price(VisitorTarget item)
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

    public Dictionary<string, double> EvaluateAll(List<VisitorTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
