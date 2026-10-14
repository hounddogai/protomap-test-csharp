namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive holiday claim calculation.</summary>
public interface IAdaptiveHolidayClaimAdjuster
{
    string Describe();
    double Adjust(AdaptiveHolidayClaim item, double valueValue);
}

public sealed record AdaptiveHolidayClaimAttemptsOffsetAdjuster(double Weight) : IAdaptiveHolidayClaimAdjuster
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(AdaptiveHolidayClaim item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

public sealed record CountryAdaptiveHolidayClaimAdjuster(Dictionary<string, double> Factors) : IAdaptiveHolidayClaimAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdaptiveHolidayClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAdaptiveHolidayClaimAdjuster(double Step) : IAdaptiveHolidayClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveHolidayClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAdaptiveHolidayClaimAdjuster(double Minimum) : IAdaptiveHolidayClaimAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveHolidayClaim item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAdaptiveHolidayClaimAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveHolidayClaimAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveHolidayClaim item, double valueValue)
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

/// <summary>Runs the adaptive holiday claim steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveHolidayClaimEngine(List<IAdaptiveHolidayClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveHolidayClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveHolidayClaimEngine DefaultAdaptiveHolidayClaimEngine()
    {
        return new AdaptiveHolidayClaimEngine(
            new List<IAdaptiveHolidayClaimAdjuster>
            {
                new AdaptiveHolidayClaimAttemptsOffsetAdjuster(1.4),
                new CountryAdaptiveHolidayClaimAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.26, ["west"] = 1.1 }),
                new RoundingAdaptiveHolidayClaimAdjuster(5.0),
                new MinimumAdaptiveHolidayClaimAdjuster(10.1),
                new TieredAdaptiveHolidayClaimAdjuster(
                    new List<double> { 5639.0, 6164.0, 8708.0 },
                    new List<double> { 1.14, 1.01, 0.97 }),
            },
            new List<string>(),
            2.7,
            89134.6);
    }

    public double Price(AdaptiveHolidayClaim item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveHolidayClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
