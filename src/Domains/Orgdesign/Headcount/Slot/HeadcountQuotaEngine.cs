namespace Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>One step of the headcount quota calculation.</summary>
public interface IHeadcountQuotaStep
{
    string Describe();
    double Adjust(HeadcountQuota item, double valueValue);
}

public sealed record HeadcountQuotaScoreOffsetAdjuster(double Weight) : IHeadcountQuotaStep
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(HeadcountQuota item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

public sealed record PercentHeadcountQuotaAdjuster(double Percent) : IHeadcountQuotaStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredHeadcountQuotaAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountQuotaStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountQuota item, double valueValue)
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

/// <summary>Runs the headcount quota steps in order and keeps a trace of each.</summary>
public sealed class HeadcountQuotaEngine(List<IHeadcountQuotaStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountQuotaStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountQuotaEngine DefaultHeadcountQuotaEngine()
    {
        return new HeadcountQuotaEngine(
            new List<IHeadcountQuotaStep>
            {
                new HeadcountQuotaScoreOffsetAdjuster(0.61),
                new PercentHeadcountQuotaAdjuster(-22.1),
                new TieredHeadcountQuotaAdjuster(
                    new List<double> { 970.0, 2798.0, 3729.0, 9116.0 },
                    new List<double> { 0.92, 1.13, 1.19, 1.19 }),
            },
            new List<string>(),
            3.6,
            41995.9);
    }

    public double Compute(HeadcountQuota item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
