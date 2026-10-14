namespace Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>One step of the global booking tier calculation.</summary>
public interface IGlobalBookingTierModifier
{
    string Describe();
    double Adjust(GlobalBookingTier item, double valueValue);
}

public sealed record CapGlobalBookingTierAdjuster(double Cap) : IGlobalBookingTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalBookingTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TitleGlobalBookingTierAdjuster(Dictionary<string, double> Factors) : IGlobalBookingTierModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(GlobalBookingTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumGlobalBookingTierAdjuster(double Minimum) : IGlobalBookingTierModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalBookingTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the global booking tier steps in order and keeps a trace of each.</summary>
public sealed class GlobalBookingTierEngine(List<IGlobalBookingTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalBookingTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalBookingTierEngine DefaultGlobalBookingTierEngine()
    {
        return new GlobalBookingTierEngine(
            new List<IGlobalBookingTierModifier>
            {
                new CapGlobalBookingTierAdjuster(31020.0),
                new TitleGlobalBookingTierAdjuster(new Dictionary<string, double> { ["south"] = 0.86 }),
                new MinimumGlobalBookingTierAdjuster(20.0),
            },
            new List<string>(),
            3.0,
            55417.6);
    }

    public double Price(GlobalBookingTier item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalBookingTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
