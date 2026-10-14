namespace Acme.Workforce.Workplace.Visitor.Bundle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor bundle calculation.</summary>
public interface IVisitorBundleAdjuster
{
    string Describe();
    double Adjust(VisitorBundle item, double valueValue);
}

public sealed record MinimumVisitorBundleAdjuster(double Minimum) : IVisitorBundleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(VisitorBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredVisitorBundleAdjuster(List<double> Bounds, List<double> Rates) : IVisitorBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorBundle item, double valueValue)
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

public sealed record VisitorBundleMinimumOffsetAdjuster(double Weight) : IVisitorBundleAdjuster
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(VisitorBundle item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingVisitorBundleAdjuster(double Step) : IVisitorBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the visitor bundle steps in order and keeps a trace of each.</summary>
public sealed class VisitorBundleEngine(List<IVisitorBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorBundleEngine DefaultVisitorBundleEngine()
    {
        return new VisitorBundleEngine(
            new List<IVisitorBundleAdjuster>
            {
                new MinimumVisitorBundleAdjuster(12.6),
                new TieredVisitorBundleAdjuster(new List<double> { 1551.0, 4868.0 }, new List<double> { 1.02, 0.94 }),
                new VisitorBundleMinimumOffsetAdjuster(1.22),
                new RoundingVisitorBundleAdjuster(1.0),
            },
            new List<string>(),
            1.8,
            45514.2);
    }

    public double Evaluate(VisitorBundle item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
