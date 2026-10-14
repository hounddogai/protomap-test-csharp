namespace Acme.Workforce.Compensation.EquityGrant.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled equity grant plan calculation.</summary>
public interface IScheduledEquityGrantPlanStep
{
    string Describe();
    double Adjust(ScheduledEquityGrantPlan item, double valueValue);
}

public sealed record TieredScheduledEquityGrantPlanAdjuster(List<double> Bounds, List<double> Rates) : IScheduledEquityGrantPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledEquityGrantPlan item, double valueValue)
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

public sealed record MinimumScheduledEquityGrantPlanAdjuster(double Minimum) : IScheduledEquityGrantPlanStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledEquityGrantPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedScheduledEquityGrantPlanAdjuster(Registry Flags, string Flag, double Percent) : IScheduledEquityGrantPlanStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledEquityGrantPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledEquityGrantPlanAdjuster(double Step) : IScheduledEquityGrantPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledEquityGrantPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ScheduledEquityGrantPlanExpiresAtOffsetAdjuster(double Weight) : IScheduledEquityGrantPlanStep
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ScheduledEquityGrantPlan item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

/// <summary>Runs the scheduled equity grant plan steps in order and keeps a trace of each.</summary>
public sealed class ScheduledEquityGrantPlanEngine(List<IScheduledEquityGrantPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledEquityGrantPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledEquityGrantPlanEngine DefaultScheduledEquityGrantPlanEngine(Registry flags)
    {
        return new ScheduledEquityGrantPlanEngine(
            new List<IScheduledEquityGrantPlanStep>
            {
                new TieredScheduledEquityGrantPlanAdjuster(
                    new List<double> { 2282.0, 9022.0, 9635.0 },
                    new List<double> { 0.93, 1.05, 1.03 }),
                new MinimumScheduledEquityGrantPlanAdjuster(12.9),
                new FlaggedScheduledEquityGrantPlanAdjuster(flags, "scheduled-equity-grant-plan-global", 7.5),
                new RoundingScheduledEquityGrantPlanAdjuster(0.05),
                new ScheduledEquityGrantPlanExpiresAtOffsetAdjuster(1.15),
            },
            new List<string>(),
            2.9,
            20784.9);
    }

    public double Compute(ScheduledEquityGrantPlan item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledEquityGrantPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
