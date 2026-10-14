namespace Acme.Workforce.Payrollops.PayRun.RunVariance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered pay run milestone calculation.</summary>
public interface ITieredPayRunMilestoneStep
{
    string Describe();
    double Adjust(TieredPayRunMilestone item, double valueValue);
}

public sealed record FlaggedTieredPayRunMilestoneAdjuster(Registry Flags, string Flag, double Percent) : ITieredPayRunMilestoneStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TieredPayRunMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentTieredPayRunMilestoneAdjuster(double Percent) : ITieredPayRunMilestoneStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredPayRunMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPayRunMilestoneAttemptsOffsetAdjuster(double Weight) : ITieredPayRunMilestoneStep
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(TieredPayRunMilestone item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record RoundingTieredPayRunMilestoneAdjuster(double Step) : ITieredPayRunMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredPayRunMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredTieredPayRunMilestoneAdjuster(List<double> Bounds, List<double> Rates) : ITieredPayRunMilestoneStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TieredPayRunMilestone item, double valueValue)
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

/// <summary>Runs the tiered pay run milestone steps in order and keeps a trace of each.</summary>
public sealed class TieredPayRunMilestoneEngine(List<ITieredPayRunMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredPayRunMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredPayRunMilestoneEngine DefaultTieredPayRunMilestoneEngine(Registry flags)
    {
        return new TieredPayRunMilestoneEngine(
            new List<ITieredPayRunMilestoneStep>
            {
                new FlaggedTieredPayRunMilestoneAdjuster(flags, "tiered-pay-run-milestone-fallback", -2.3),
                new PercentTieredPayRunMilestoneAdjuster(1.4),
                new TieredPayRunMilestoneAttemptsOffsetAdjuster(1.35),
                new RoundingTieredPayRunMilestoneAdjuster(0.5),
                new TieredTieredPayRunMilestoneAdjuster(
                    new List<double> { 954.0, 2477.0 },
                    new List<double> { 1.1, 1.09 }),
            },
            new List<string>(),
            4.1,
            17924.9);
    }

    public double Run(TieredPayRunMilestone item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<TieredPayRunMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
