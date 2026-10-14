namespace Acme.Workforce.Payrollops.Garnishment.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled garnishment plan calculation.</summary>
public interface IScheduledGarnishmentPlanModifier
{
    string Describe();
    double Adjust(ScheduledGarnishmentPlan item, double valueValue);
}

public sealed record ChannelScheduledGarnishmentPlanAdjuster(Dictionary<string, double> Factors) : IScheduledGarnishmentPlanModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(ScheduledGarnishmentPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScheduledGarnishmentPlanAdjuster(List<double> Bounds, List<double> Rates) : IScheduledGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledGarnishmentPlan item, double valueValue)
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

public sealed record FlaggedScheduledGarnishmentPlanAdjuster(Registry Flags, string Flag, double Percent) : IScheduledGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledGarnishmentPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledGarnishmentPlanAdjuster(double Step) : IScheduledGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledGarnishmentPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumScheduledGarnishmentPlanAdjuster(double Minimum) : IScheduledGarnishmentPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledGarnishmentPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled garnishment plan steps in order and keeps a trace of each.</summary>
public sealed class ScheduledGarnishmentPlanEngine(List<IScheduledGarnishmentPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledGarnishmentPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledGarnishmentPlanEngine DefaultScheduledGarnishmentPlanEngine(Registry flags)
    {
        return new ScheduledGarnishmentPlanEngine(
            new List<IScheduledGarnishmentPlanModifier>
            {
                new ChannelScheduledGarnishmentPlanAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.32, ["south"] = 0.95, ["north"] = 0.77 }),
                new TieredScheduledGarnishmentPlanAdjuster(
                    new List<double> { 1692.0, 6241.0, 6658.0, 7700.0 },
                    new List<double> { 1.12, 1.07, 1.07, 0.94 }),
                new FlaggedScheduledGarnishmentPlanAdjuster(flags, "scheduled-garnishment-plan-weighted", -2.2),
                new RoundingScheduledGarnishmentPlanAdjuster(5.0),
                new MinimumScheduledGarnishmentPlanAdjuster(17.3),
            },
            new List<string>(),
            2.3,
            27217.6);
    }

    public double Price(ScheduledGarnishmentPlan item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledGarnishmentPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
