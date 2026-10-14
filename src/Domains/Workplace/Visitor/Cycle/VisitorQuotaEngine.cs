namespace Acme.Workforce.Workplace.Visitor.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the visitor quota calculation.</summary>
public interface IVisitorQuotaModifier
{
    string Describe();
    double Adjust(VisitorQuota item, double valueValue);
}

public sealed record TieredVisitorQuotaAdjuster(List<double> Bounds, List<double> Rates) : IVisitorQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(VisitorQuota item, double valueValue)
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

public sealed record PercentVisitorQuotaAdjuster(double Percent) : IVisitorQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(VisitorQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingVisitorQuotaAdjuster(double Step) : IVisitorQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(VisitorQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the visitor quota steps in order and keeps a trace of each.</summary>
public sealed class VisitorQuotaEngine(List<IVisitorQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IVisitorQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static VisitorQuotaEngine DefaultVisitorQuotaEngine()
    {
        return new VisitorQuotaEngine(
            new List<IVisitorQuotaModifier>
            {
                new TieredVisitorQuotaAdjuster(
                    new List<double> { 2861.0, 3290.0, 3309.0, 3910.0 },
                    new List<double> { 1.03, 1.1, 1.01, 0.99 }),
                new PercentVisitorQuotaAdjuster(-20.7),
                new RoundingVisitorQuotaAdjuster(0.25),
            },
            new List<string>(),
            4.9,
            39799.4);
    }

    public double Compute(VisitorQuota item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<VisitorQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
