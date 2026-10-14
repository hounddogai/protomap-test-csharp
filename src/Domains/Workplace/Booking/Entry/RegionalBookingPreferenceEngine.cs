namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional booking preference calculation.</summary>
public interface IRegionalBookingPreferenceAdjuster
{
    string Describe();
    double Adjust(RegionalBookingPreference item, double valueValue);
}

public sealed record RegionalBookingPreferenceCeilingOffsetAdjuster(double Weight) : IRegionalBookingPreferenceAdjuster
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(RegionalBookingPreference item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record PercentRegionalBookingPreferenceAdjuster(double Percent) : IRegionalBookingPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalBookingPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalBookingPreferenceAdjuster(double Minimum) : IRegionalBookingPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalBookingPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRegionalBookingPreferenceAdjuster(double Step) : IRegionalBookingPreferenceAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalBookingPreference item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalBookingPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IRegionalBookingPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalBookingPreference item, double valueValue)
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

public sealed record CapRegionalBookingPreferenceAdjuster(double Cap) : IRegionalBookingPreferenceAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalBookingPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the regional booking preference steps in order and keeps a trace of each.</summary>
public sealed class RegionalBookingPreferenceEngine(List<IRegionalBookingPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalBookingPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalBookingPreferenceEngine DefaultRegionalBookingPreferenceEngine()
    {
        return new RegionalBookingPreferenceEngine(
            new List<IRegionalBookingPreferenceAdjuster>
            {
                new RegionalBookingPreferenceCeilingOffsetAdjuster(0.76),
                new PercentRegionalBookingPreferenceAdjuster(9.9),
                new MinimumRegionalBookingPreferenceAdjuster(30.8),
                new RoundingRegionalBookingPreferenceAdjuster(0.5),
                new TieredRegionalBookingPreferenceAdjuster(
                    new List<double> { 8254.0, 9276.0 },
                    new List<double> { 0.91, 0.99 }),
                new CapRegionalBookingPreferenceAdjuster(47169.0),
            },
            new List<string>(),
            1.0,
            83348.2);
    }

    public double Run(RegionalBookingPreference item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalBookingPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
