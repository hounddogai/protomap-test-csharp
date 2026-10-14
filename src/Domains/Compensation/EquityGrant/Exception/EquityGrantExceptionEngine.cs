namespace Acme.Workforce.Compensation.EquityGrant.Exception;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the equity grant exception calculation.</summary>
public interface IEquityGrantExceptionFactor
{
    string Describe();
    double Adjust(EquityGrantException item, double valueValue);
}

public sealed record SourceEquityGrantExceptionAdjuster(Dictionary<string, double> Factors) : IEquityGrantExceptionFactor
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(EquityGrantException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredEquityGrantExceptionAdjuster(List<double> Bounds, List<double> Rates) : IEquityGrantExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EquityGrantException item, double valueValue)
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

public sealed record FlaggedEquityGrantExceptionAdjuster(Registry Flags, string Flag, double Percent) : IEquityGrantExceptionFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EquityGrantException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the equity grant exception steps in order and keeps a trace of each.</summary>
public sealed class EquityGrantExceptionEngine(List<IEquityGrantExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEquityGrantExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EquityGrantExceptionEngine DefaultEquityGrantExceptionEngine(Registry flags)
    {
        return new EquityGrantExceptionEngine(
            new List<IEquityGrantExceptionFactor>
            {
                new SourceEquityGrantExceptionAdjuster(new Dictionary<string, double> { ["retail"] = 0.96 }),
                new TieredEquityGrantExceptionAdjuster(
                    new List<double> { 1339.0, 2511.0, 7454.0, 9986.0 },
                    new List<double> { 1.2, 1.13, 1.16, 0.91 }),
                new FlaggedEquityGrantExceptionAdjuster(flags, "equity-grant-exception-fallback", 8.1),
            },
            new List<string>(),
            0.9,
            81872.4);
    }

    public double Run(EquityGrantException item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<EquityGrantException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
