namespace Acme.Workforce.Assets.Refresh.Batch;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the refresh claim calculation.</summary>
public interface IRefreshClaimStep
{
    string Describe();
    double Adjust(RefreshClaim item, double valueValue);
}

public sealed record TieredRefreshClaimAdjuster(List<double> Bounds, List<double> Rates) : IRefreshClaimStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshClaim item, double valueValue)
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

public sealed record TitleRefreshClaimAdjuster(Dictionary<string, double> Factors) : IRefreshClaimStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RefreshClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRefreshClaimAdjuster(Registry Flags, string Flag, double Percent) : IRefreshClaimStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshClaim item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the refresh claim steps in order and keeps a trace of each.</summary>
public sealed class RefreshClaimEngine(List<IRefreshClaimStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshClaimStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshClaimEngine DefaultRefreshClaimEngine(Registry flags)
    {
        return new RefreshClaimEngine(
            new List<IRefreshClaimStep>
            {
                new TieredRefreshClaimAdjuster(
                    new List<double> { 708.0, 1690.0, 8129.0, 8858.0 },
                    new List<double> { 1.08, 1.11, 1.03, 1.14 }),
                new TitleRefreshClaimAdjuster(new Dictionary<string, double> { ["north"] = 0.79 }),
                new FlaggedRefreshClaimAdjuster(flags, "refresh-claim-regional", 1.0),
            },
            new List<string>(),
            1.4,
            50800.4);
    }

    public double Evaluate(RefreshClaim item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
