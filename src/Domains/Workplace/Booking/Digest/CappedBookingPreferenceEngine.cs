namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped booking preference calculation.</summary>
public interface ICappedBookingPreferenceModifier
{
    string Describe();
    double Adjust(CappedBookingPreference item, double valueValue);
}

public sealed record PercentCappedBookingPreferenceAdjuster(double Percent) : ICappedBookingPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedBookingPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedBookingPreferenceAdjuster(double Step) : ICappedBookingPreferenceModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedBookingPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredCappedBookingPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ICappedBookingPreferenceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedBookingPreference item, double valueValue)
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

public sealed record MinimumCappedBookingPreferenceAdjuster(double Minimum) : ICappedBookingPreferenceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedBookingPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record UnitCappedBookingPreferenceAdjuster(Dictionary<string, double> Factors) : ICappedBookingPreferenceModifier
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(CappedBookingPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedBookingPreferenceAdjuster(double Cap) : ICappedBookingPreferenceModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedBookingPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the capped booking preference steps in order and keeps a trace of each.</summary>
public sealed class CappedBookingPreferenceEngine(List<ICappedBookingPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedBookingPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedBookingPreferenceEngine DefaultCappedBookingPreferenceEngine()
    {
        return new CappedBookingPreferenceEngine(
            new List<ICappedBookingPreferenceModifier>
            {
                new PercentCappedBookingPreferenceAdjuster(-18.5),
                new RoundingCappedBookingPreferenceAdjuster(0.05),
                new TieredCappedBookingPreferenceAdjuster(
                    new List<double> { 227.0, 1342.0, 4409.0, 8682.0 },
                    new List<double> { 0.8, 0.87, 1.13, 0.93 }),
                new MinimumCappedBookingPreferenceAdjuster(7.4),
                new UnitCappedBookingPreferenceAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.03, ["west"] = 1.37, ["south"] = 0.71 }),
                new CapCappedBookingPreferenceAdjuster(11674.0),
            },
            new List<string>(),
            0.6,
            22133.2);
    }

    public double Evaluate(CappedBookingPreference item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<CappedBookingPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
