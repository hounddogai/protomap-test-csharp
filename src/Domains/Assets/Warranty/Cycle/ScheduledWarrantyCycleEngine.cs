namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled warranty cycle calculation.</summary>
public interface IScheduledWarrantyCycleStep
{
    string Describe();
    double Adjust(ScheduledWarrantyCycle item, double valueValue);
}

public sealed record ScheduledWarrantyCycleWeightOffsetAdjuster(double Weight) : IScheduledWarrantyCycleStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(ScheduledWarrantyCycle item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record CapScheduledWarrantyCycleAdjuster(double Cap) : IScheduledWarrantyCycleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledWarrantyCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredScheduledWarrantyCycleAdjuster(List<double> Bounds, List<double> Rates) : IScheduledWarrantyCycleStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledWarrantyCycle item, double valueValue)
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

public sealed record RoundingScheduledWarrantyCycleAdjuster(double Step) : IScheduledWarrantyCycleStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScheduledWarrantyCycle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedScheduledWarrantyCycleAdjuster(Registry Flags, string Flag, double Percent) : IScheduledWarrantyCycleStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledWarrantyCycle item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled warranty cycle steps in order and keeps a trace of each.</summary>
public sealed class ScheduledWarrantyCycleEngine(List<IScheduledWarrantyCycleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledWarrantyCycleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledWarrantyCycleEngine DefaultScheduledWarrantyCycleEngine(Registry flags)
    {
        return new ScheduledWarrantyCycleEngine(
            new List<IScheduledWarrantyCycleStep>
            {
                new ScheduledWarrantyCycleWeightOffsetAdjuster(1.33),
                new CapScheduledWarrantyCycleAdjuster(12128.0),
                new TieredScheduledWarrantyCycleAdjuster(
                    new List<double> { 2282.0, 4532.0, 7409.0 },
                    new List<double> { 1.09, 1.08, 1.15 }),
                new RoundingScheduledWarrantyCycleAdjuster(0.5),
                new FlaggedScheduledWarrantyCycleAdjuster(flags, "scheduled-warranty-cycle-seasonal", 3.4),
            },
            new List<string>(),
            0.4,
            91562.2);
    }

    public double Compute(ScheduledWarrantyCycle item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledWarrantyCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
