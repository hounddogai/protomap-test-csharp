namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled accrual baseline calculation.</summary>
public interface IScheduledAccrualBaselineStep
{
    string Describe();
    double Adjust(ScheduledAccrualBaseline item, double valueValue);
}

public sealed record CapScheduledAccrualBaselineAdjuster(double Cap) : IScheduledAccrualBaselineStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledAccrualBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedScheduledAccrualBaselineAdjuster(Registry Flags, string Flag, double Percent) : IScheduledAccrualBaselineStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledAccrualBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumScheduledAccrualBaselineAdjuster(double Minimum) : IScheduledAccrualBaselineStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAccrualBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledAccrualBaselineAdjuster(double Percent) : IScheduledAccrualBaselineStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledAccrualBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingScheduledAccrualBaselineAdjuster(double Step) : IScheduledAccrualBaselineStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledAccrualBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scheduled accrual baseline steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAccrualBaselineEngine(List<IScheduledAccrualBaselineStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAccrualBaselineStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAccrualBaselineEngine DefaultScheduledAccrualBaselineEngine(Registry flags)
    {
        return new ScheduledAccrualBaselineEngine(
            new List<IScheduledAccrualBaselineStep>
            {
                new CapScheduledAccrualBaselineAdjuster(37375.0),
                new FlaggedScheduledAccrualBaselineAdjuster(flags, "scheduled-accrual-baseline-pending", 4.6),
                new MinimumScheduledAccrualBaselineAdjuster(0.7),
                new PercentScheduledAccrualBaselineAdjuster(-10.0),
                new RoundingScheduledAccrualBaselineAdjuster(0.5),
            },
            new List<string>(),
            4.7,
            44321.7);
    }

    public double Price(ScheduledAccrualBaseline item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAccrualBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
