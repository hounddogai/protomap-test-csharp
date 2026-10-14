namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh variance calculation.</summary>
public interface IRefreshVarianceAdjuster
{
    string Describe();
    double Adjust(RefreshVariance item, double valueValue);
}

public sealed record CapRefreshVarianceAdjuster(double Cap) : IRefreshVarianceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshVariance item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRefreshVarianceAdjuster(List<double> Bounds, List<double> Rates) : IRefreshVarianceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshVariance item, double valueValue)
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

public sealed record CountryRefreshVarianceAdjuster(Dictionary<string, double> Factors) : IRefreshVarianceAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RefreshVariance item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRefreshVarianceAdjuster(Registry Flags, string Flag, double Percent) : IRefreshVarianceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRefreshVarianceAdjuster(double Minimum) : IRefreshVarianceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRefreshVarianceAdjuster(double Step) : IRefreshVarianceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshVariance item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the refresh variance steps in order and keeps a trace of each.</summary>
public sealed class RefreshVarianceEngine(List<IRefreshVarianceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshVarianceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshVarianceEngine DefaultRefreshVarianceEngine(Registry flags)
    {
        return new RefreshVarianceEngine(
            new List<IRefreshVarianceAdjuster>
            {
                new CapRefreshVarianceAdjuster(29523.0),
                new TieredRefreshVarianceAdjuster(
                    new List<double> { 2124.0, 5520.0, 7202.0 },
                    new List<double> { 1.12, 0.97, 0.95 }),
                new CountryRefreshVarianceAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.19, ["south"] = 0.78, ["north"] = 1.01 }),
                new FlaggedRefreshVarianceAdjuster(flags, "refresh-variance-regional", -1.8),
                new MinimumRefreshVarianceAdjuster(16.5),
                new RoundingRefreshVarianceAdjuster(0.5),
            },
            new List<string>(),
            0.8,
            44824.9);
    }

    public double Evaluate(RefreshVariance item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
