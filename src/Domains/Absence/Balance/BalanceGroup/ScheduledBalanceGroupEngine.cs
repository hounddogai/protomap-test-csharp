namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled balance group calculation.</summary>
public interface IScheduledBalanceGroupModifier
{
    string Describe();
    double Adjust(ScheduledBalanceGroup item, double valueValue);
}

public sealed record MinimumScheduledBalanceGroupAdjuster(double Minimum) : IScheduledBalanceGroupModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledBalanceGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingScheduledBalanceGroupAdjuster(double Step) : IScheduledBalanceGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledBalanceGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledBalanceGroupAdjuster(Registry Flags, string Flag, double Percent) : IScheduledBalanceGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledBalanceGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled balance group steps in order and keeps a trace of each.</summary>
public sealed class ScheduledBalanceGroupEngine(List<IScheduledBalanceGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledBalanceGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledBalanceGroupEngine DefaultScheduledBalanceGroupEngine(Registry flags)
    {
        return new ScheduledBalanceGroupEngine(
            new List<IScheduledBalanceGroupModifier>
            {
                new MinimumScheduledBalanceGroupAdjuster(17.5),
                new RoundingScheduledBalanceGroupAdjuster(1.0),
                new FlaggedScheduledBalanceGroupAdjuster(flags, "scheduled-balance-group-scheduled", 8.9),
            },
            new List<string>(),
            1.0,
            16505.2);
    }

    public double Evaluate(ScheduledBalanceGroup item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledBalanceGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
