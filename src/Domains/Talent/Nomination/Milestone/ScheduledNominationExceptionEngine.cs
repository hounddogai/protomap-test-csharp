namespace Acme.Workforce.Talent.Nomination.Milestone;

/// <summary>One step of the scheduled nomination exception calculation.</summary>
public interface IScheduledNominationExceptionFactor
{
    string Describe();
    double Adjust(ScheduledNominationException item, double valueValue);
}

public sealed record MinimumScheduledNominationExceptionAdjuster(double Minimum) : IScheduledNominationExceptionFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledNominationException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScheduledNominationExceptionAdjuster(double Cap) : IScheduledNominationExceptionFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledNominationException item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleScheduledNominationExceptionAdjuster(Dictionary<string, double> Factors) : IScheduledNominationExceptionFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScheduledNominationException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScheduledNominationExceptionAdjuster(double Percent) : IScheduledNominationExceptionFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledNominationException item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredScheduledNominationExceptionAdjuster(List<double> Bounds, List<double> Rates) : IScheduledNominationExceptionFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledNominationException item, double valueValue)
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

/// <summary>Runs the scheduled nomination exception steps in order and keeps a trace of each.</summary>
public sealed class ScheduledNominationExceptionEngine(List<IScheduledNominationExceptionFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledNominationExceptionFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledNominationExceptionEngine DefaultScheduledNominationExceptionEngine()
    {
        return new ScheduledNominationExceptionEngine(
            new List<IScheduledNominationExceptionFactor>
            {
                new MinimumScheduledNominationExceptionAdjuster(19.6),
                new CapScheduledNominationExceptionAdjuster(37828.0),
                new TitleScheduledNominationExceptionAdjuster(new Dictionary<string, double> { ["east"] = 0.94 }),
                new PercentScheduledNominationExceptionAdjuster(13.1),
                new TieredScheduledNominationExceptionAdjuster(
                    new List<double> { 4099.0, 4180.0, 9053.0 },
                    new List<double> { 1.09, 0.87, 1.13 }),
            },
            new List<string>(),
            3.9,
            97246.5);
    }

    public double Run(ScheduledNominationException item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledNominationException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
