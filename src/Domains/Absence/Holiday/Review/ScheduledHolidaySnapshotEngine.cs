namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled holiday snapshot calculation.</summary>
public interface IScheduledHolidaySnapshotStep
{
    string Describe();
    double Adjust(ScheduledHolidaySnapshot item, double valueValue);
}

public sealed record TieredScheduledHolidaySnapshotAdjuster(List<double> Bounds, List<double> Rates) : IScheduledHolidaySnapshotStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledHolidaySnapshot item, double valueValue)
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

public sealed record PercentScheduledHolidaySnapshotAdjuster(double Percent) : IScheduledHolidaySnapshotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledHolidaySnapshot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryScheduledHolidaySnapshotAdjuster(Dictionary<string, double> Factors) : IScheduledHolidaySnapshotStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ScheduledHolidaySnapshot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumScheduledHolidaySnapshotAdjuster(double Minimum) : IScheduledHolidaySnapshotStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledHolidaySnapshot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingScheduledHolidaySnapshotAdjuster(double Step) : IScheduledHolidaySnapshotStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledHolidaySnapshot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapScheduledHolidaySnapshotAdjuster(double Cap) : IScheduledHolidaySnapshotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledHolidaySnapshot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the scheduled holiday snapshot steps in order and keeps a trace of each.</summary>
public sealed class ScheduledHolidaySnapshotEngine(List<IScheduledHolidaySnapshotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledHolidaySnapshotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledHolidaySnapshotEngine DefaultScheduledHolidaySnapshotEngine()
    {
        return new ScheduledHolidaySnapshotEngine(
            new List<IScheduledHolidaySnapshotStep>
            {
                new TieredScheduledHolidaySnapshotAdjuster(
                    new List<double> { 7341.0, 9132.0 },
                    new List<double> { 0.88, 1.04 }),
                new PercentScheduledHolidaySnapshotAdjuster(13.0),
                new CountryScheduledHolidaySnapshotAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.2, ["south"] = 1.3 }),
                new MinimumScheduledHolidaySnapshotAdjuster(10.2),
                new RoundingScheduledHolidaySnapshotAdjuster(5.0),
                new CapScheduledHolidaySnapshotAdjuster(35398.0),
            },
            new List<string>(),
            0.2,
            20967.4);
    }

    public double Compute(ScheduledHolidaySnapshot item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledHolidaySnapshot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
