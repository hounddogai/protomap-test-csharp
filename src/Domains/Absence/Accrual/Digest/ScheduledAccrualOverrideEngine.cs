namespace Acme.Workforce.Absence.Accrual.Digest;

/// <summary>One step of the scheduled accrual override calculation.</summary>
public interface IScheduledAccrualOverrideAdjuster
{
    string Describe();
    double Adjust(ScheduledAccrualOverride item, double valueValue);
}

public sealed record ScheduledAccrualOverrideRatingOffsetAdjuster(double Weight) : IScheduledAccrualOverrideAdjuster
{
    public string Describe()
    {
        return $"plus rating times {Weight}";
    }

    public double Adjust(ScheduledAccrualOverride item, double valueValue)
    {
        return valueValue + item.Rating * Weight;
    }
}

public sealed record CapScheduledAccrualOverrideAdjuster(double Cap) : IScheduledAccrualOverrideAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledAccrualOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScheduledAccrualOverrideAdjuster(double Minimum) : IScheduledAccrualOverrideAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledAccrualOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentScheduledAccrualOverrideAdjuster(double Percent) : IScheduledAccrualOverrideAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScheduledAccrualOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scheduled accrual override steps in order and keeps a trace of each.</summary>
public sealed class ScheduledAccrualOverrideEngine(List<IScheduledAccrualOverrideAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledAccrualOverrideAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledAccrualOverrideEngine DefaultScheduledAccrualOverrideEngine()
    {
        return new ScheduledAccrualOverrideEngine(
            new List<IScheduledAccrualOverrideAdjuster>
            {
                new ScheduledAccrualOverrideRatingOffsetAdjuster(0.88),
                new CapScheduledAccrualOverrideAdjuster(17468.0),
                new MinimumScheduledAccrualOverrideAdjuster(17.9),
                new PercentScheduledAccrualOverrideAdjuster(-1.7),
            },
            new List<string>(),
            4.5,
            79562.1);
    }

    public double Price(ScheduledAccrualOverride item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledAccrualOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
