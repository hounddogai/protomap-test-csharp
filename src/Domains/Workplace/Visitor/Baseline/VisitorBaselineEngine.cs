namespace Acme.Workforce.Workplace.Visitor.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor baseline calculation.</summary>
public interface IVisitorBaselineStep
{
    string Describe();
    double Adjust(VisitorBaseline item, double valueValue);
}

public sealed record RoundingVisitorBaselineAdjuster(double Step) : IVisitorBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentVisitorBaselineAdjuster(double Percent) : IVisitorBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredVisitorBaselineAdjuster(List<double> Bounds, List<double> Rates) : IVisitorBaselineStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorBaseline item, double valueValue)
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

/// <summary>Runs the visitor baseline steps in order and keeps a trace of each.</summary>
public sealed class VisitorBaselineEngine(List<IVisitorBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorBaselineEngine DefaultVisitorBaselineEngine()
    {
        return new VisitorBaselineEngine(
            new List<IVisitorBaselineStep>
            {
                new RoundingVisitorBaselineAdjuster(1.0),
                new PercentVisitorBaselineAdjuster(-19.4),
                new TieredVisitorBaselineAdjuster(
                    new List<double> { 6341.0, 6760.0, 8983.0 },
                    new List<double> { 0.9, 1.01, 1.01 }),
            },
            new List<string>(),
            0.1,
            15169.9);
    }

    public double Run(VisitorBaseline item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
