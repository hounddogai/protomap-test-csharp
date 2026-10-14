namespace Acme.Workforce.Payrollops.RetroPay.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled retro pay entry calculation.</summary>
public interface IScheduledRetroPayEntryAdjuster
{
    string Describe();
    double Adjust(ScheduledRetroPayEntry item, double valueValue);
}

public sealed record ScheduledRetroPayEntryLeaveDaysOffsetAdjuster(double Weight) : IScheduledRetroPayEntryAdjuster
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(ScheduledRetroPayEntry item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

public sealed record TitleScheduledRetroPayEntryAdjuster(Dictionary<string, double> Factors) : IScheduledRetroPayEntryAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScheduledRetroPayEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScheduledRetroPayEntryAdjuster(double Cap) : IScheduledRetroPayEntryAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledRetroPayEntry item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledRetroPayEntryAdjuster(double Minimum) : IScheduledRetroPayEntryAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledRetroPayEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedScheduledRetroPayEntryAdjuster(Registry Flags, string Flag, double Percent) : IScheduledRetroPayEntryAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledRetroPayEntry item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledRetroPayEntryAdjuster(List<double> Bounds, List<double> Rates) : IScheduledRetroPayEntryAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledRetroPayEntry item, double valueValue)
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

/// <summary>Runs the scheduled retro pay entry steps in order and keeps a trace of each.</summary>
public sealed class ScheduledRetroPayEntryEngine(List<IScheduledRetroPayEntryAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledRetroPayEntryAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledRetroPayEntryEngine DefaultScheduledRetroPayEntryEngine(Registry flags)
    {
        return new ScheduledRetroPayEntryEngine(
            new List<IScheduledRetroPayEntryAdjuster>
            {
                new ScheduledRetroPayEntryLeaveDaysOffsetAdjuster(1.96),
                new TitleScheduledRetroPayEntryAdjuster(new Dictionary<string, double> { ["central"] = 0.94 }),
                new CapScheduledRetroPayEntryAdjuster(35828.0),
                new MinimumScheduledRetroPayEntryAdjuster(47.8),
                new FlaggedScheduledRetroPayEntryAdjuster(flags, "scheduled-retro-pay-entry-adaptive", -3.7),
                new TieredScheduledRetroPayEntryAdjuster(
                    new List<double> { 4953.0, 5158.0, 8318.0, 8861.0 },
                    new List<double> { 1.0, 0.9, 0.97, 0.96 }),
            },
            new List<string>(),
            1.9,
            87770.2);
    }

    public double Evaluate(ScheduledRetroPayEntry item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledRetroPayEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
