namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the engagement record calculation.</summary>
public interface IEngagementRecordFactor
{
    string Describe();
    double Adjust(EngagementRecord item, double valueValue);
}

public sealed record FlaggedEngagementRecordAdjuster(Registry Flags, string Flag, double Percent) : IEngagementRecordFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(EngagementRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapEngagementRecordAdjuster(double Cap) : IEngagementRecordFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(EngagementRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentEngagementRecordAdjuster(double Percent) : IEngagementRecordFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(EngagementRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record EngagementRecordDurationMinutesOffsetAdjuster(double Weight) : IEngagementRecordFactor
{
    public string Describe()
    {
        return $"plus duration minutes times {Weight}";
    }

    public double Adjust(EngagementRecord item, double valueValue)
    {
        return valueValue + (double)item.DurationMinutes * Weight;
    }
}

public sealed record TieredEngagementRecordAdjuster(List<double> Bounds, List<double> Rates) : IEngagementRecordFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(EngagementRecord item, double valueValue)
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

public sealed record MinimumEngagementRecordAdjuster(double Minimum) : IEngagementRecordFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(EngagementRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the engagement record steps in order and keeps a trace of each.</summary>
public sealed class EngagementRecordEngine(List<IEngagementRecordFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IEngagementRecordFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static EngagementRecordEngine DefaultEngagementRecordEngine(Registry flags)
    {
        return new EngagementRecordEngine(
            new List<IEngagementRecordFactor>
            {
                new FlaggedEngagementRecordAdjuster(flags, "engagement-record-regional", 8.2),
                new CapEngagementRecordAdjuster(32502.0),
                new PercentEngagementRecordAdjuster(-18.6),
                new EngagementRecordDurationMinutesOffsetAdjuster(1.23),
                new TieredEngagementRecordAdjuster(
                    new List<double> { 429.0, 1218.0 },
                    new List<double> { 0.9, 0.86 }),
                new MinimumEngagementRecordAdjuster(17.8),
            },
            new List<string>(),
            2.0,
            37410.4);
    }

    public double Evaluate(EngagementRecord item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<EngagementRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
