namespace Acme.Workforce.Engagement.TownHall.HallException;

/// <summary>One step of the scheduled town hall exception calculation.</summary>
public interface IScheduledTownHallExceptionStep
{
    string Describe();
    double Adjust(ScheduledTownHallException item, double valueValue);
}

public sealed record CapScheduledTownHallExceptionAdjuster(double Cap) : IScheduledTownHallExceptionStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledTownHallException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredScheduledTownHallExceptionAdjuster(List<double> Bounds, List<double> Rates) : IScheduledTownHallExceptionStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledTownHallException item, double valueValue)
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

public sealed record MinimumScheduledTownHallExceptionAdjuster(double Minimum) : IScheduledTownHallExceptionStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledTownHallException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scheduled town hall exception steps in order and keeps a trace of each.</summary>
public sealed class ScheduledTownHallExceptionEngine(List<IScheduledTownHallExceptionStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledTownHallExceptionStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledTownHallExceptionEngine DefaultScheduledTownHallExceptionEngine()
    {
        return new ScheduledTownHallExceptionEngine(
            new List<IScheduledTownHallExceptionStep>
            {
                new CapScheduledTownHallExceptionAdjuster(21319.0),
                new TieredScheduledTownHallExceptionAdjuster(
                    new List<double> { 478.0, 7483.0, 8291.0, 9043.0 },
                    new List<double> { 0.93, 1.18, 1.07, 0.83 }),
                new MinimumScheduledTownHallExceptionAdjuster(44.6),
            },
            new List<string>(),
            1.2,
            48573.4);
    }

    public double Evaluate(ScheduledTownHallException item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledTownHallException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
