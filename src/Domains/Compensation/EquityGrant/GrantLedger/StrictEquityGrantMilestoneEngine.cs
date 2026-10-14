namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the strict equity grant milestone calculation.</summary>
public interface IStrictEquityGrantMilestoneStep
{
    string Describe();
    double Adjust(StrictEquityGrantMilestone item, double valueValue);
}

public sealed record CapStrictEquityGrantMilestoneAdjuster(double Cap) : IStrictEquityGrantMilestoneStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictEquityGrantMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingStrictEquityGrantMilestoneAdjuster(double Step) : IStrictEquityGrantMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(StrictEquityGrantMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionStrictEquityGrantMilestoneAdjuster(Dictionary<string, double> Factors) : IStrictEquityGrantMilestoneStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(StrictEquityGrantMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumStrictEquityGrantMilestoneAdjuster(double Minimum) : IStrictEquityGrantMilestoneStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(StrictEquityGrantMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredStrictEquityGrantMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IStrictEquityGrantMilestoneStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictEquityGrantMilestone item, double valueValue)
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

public sealed record FlaggedStrictEquityGrantMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IStrictEquityGrantMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(StrictEquityGrantMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the strict equity grant milestone steps in order and keeps a trace of each.</summary>
public sealed class StrictEquityGrantMilestoneEngine(List<IStrictEquityGrantMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictEquityGrantMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictEquityGrantMilestoneEngine DefaultStrictEquityGrantMilestoneEngine(Registry flags)
    {
        return new StrictEquityGrantMilestoneEngine(
            new List<IStrictEquityGrantMilestoneStep>
            {
                new CapStrictEquityGrantMilestoneAdjuster(11836.0),
                new RoundingStrictEquityGrantMilestoneAdjuster(0.05),
                new RegionStrictEquityGrantMilestoneAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.97, ["retail"] = 1.26 }),
                new MinimumStrictEquityGrantMilestoneAdjuster(32.9),
                new TieredStrictEquityGrantMilestoneAdjuster(
                    new List<double> { 5713.0, 6433.0 },
                    new List<double> { 1.06, 0.88 }),
                new FlaggedStrictEquityGrantMilestoneAdjuster(flags, "strict-equity-grant-milestone-primary", -8.8),
            },
            new List<string>(),
            1.1,
            74312.3);
    }

    public double Price(StrictEquityGrantMilestone item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<StrictEquityGrantMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
