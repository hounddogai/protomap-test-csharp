namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending equity grant variance calculation.</summary>
public interface IPendingEquityGrantVarianceAdjuster
{
    string Describe();
    double Adjust(PendingEquityGrantVariance item, double valueValue);
}

public sealed record SourcePendingEquityGrantVarianceAdjuster(Dictionary<string, double> Factors) : IPendingEquityGrantVarianceAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PendingEquityGrantVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPendingEquityGrantVarianceAdjuster(double Percent) : IPendingEquityGrantVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingEquityGrantVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingEquityGrantVarianceAdjuster(double Step) : IPendingEquityGrantVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingEquityGrantVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending equity grant variance steps in order and keeps a trace of each.</summary>
public sealed class PendingEquityGrantVarianceEngine(List<IPendingEquityGrantVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingEquityGrantVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingEquityGrantVarianceEngine DefaultPendingEquityGrantVarianceEngine()
    {
        return new PendingEquityGrantVarianceEngine(
            new List<IPendingEquityGrantVarianceAdjuster>
            {
                new SourcePendingEquityGrantVarianceAdjuster(new Dictionary<string, double> { ["online"] = 0.86 }),
                new PercentPendingEquityGrantVarianceAdjuster(-5.3),
                new RoundingPendingEquityGrantVarianceAdjuster(0.5),
            },
            new List<string>(),
            3.7,
            92098.2);
    }

    public double Compute(PendingEquityGrantVariance item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PendingEquityGrantVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
