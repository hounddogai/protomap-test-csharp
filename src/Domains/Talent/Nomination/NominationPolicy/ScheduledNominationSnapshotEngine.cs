namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled nomination snapshot calculation.</summary>
public interface IScheduledNominationSnapshotAdjuster
{
    string Describe();
    double Adjust(ScheduledNominationSnapshot item, double valueValue);
}

public sealed record MinimumScheduledNominationSnapshotAdjuster(double Minimum) : IScheduledNominationSnapshotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledNominationSnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledNominationSnapshotAdjuster(double Cap) : IScheduledNominationSnapshotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledNominationSnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredScheduledNominationSnapshotAdjuster(List<double> Bounds, List<double> Rates) : IScheduledNominationSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledNominationSnapshot item, double valueValue)
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

public sealed record PercentScheduledNominationSnapshotAdjuster(double Percent) : IScheduledNominationSnapshotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledNominationSnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleScheduledNominationSnapshotAdjuster(Dictionary<string, double> Factors) : IScheduledNominationSnapshotAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(ScheduledNominationSnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scheduled nomination snapshot steps in order and keeps a trace of each.</summary>
public sealed class ScheduledNominationSnapshotEngine(List<IScheduledNominationSnapshotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledNominationSnapshotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledNominationSnapshotEngine DefaultScheduledNominationSnapshotEngine()
    {
        return new ScheduledNominationSnapshotEngine(
            new List<IScheduledNominationSnapshotAdjuster>
            {
                new MinimumScheduledNominationSnapshotAdjuster(7.6),
                new CapScheduledNominationSnapshotAdjuster(14334.0),
                new TieredScheduledNominationSnapshotAdjuster(
                    new List<double> { 1400.0, 4811.0, 5096.0, 9911.0 },
                    new List<double> { 1.05, 1.07, 0.89, 0.95 }),
                new PercentScheduledNominationSnapshotAdjuster(-13.2),
                new LocaleScheduledNominationSnapshotAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.17, ["west"] = 1.13 }),
            },
            new List<string>(),
            0.7,
            44149.4);
    }

    public double Evaluate(ScheduledNominationSnapshot item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledNominationSnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
