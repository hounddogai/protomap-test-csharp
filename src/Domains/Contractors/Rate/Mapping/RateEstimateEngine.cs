namespace Acme.Workforce.Contractors.Rate.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate estimate calculation.</summary>
public interface IRateEstimateModifier
{
    string Describe();
    double Adjust(RateEstimate item, double valueValue);
}

public sealed record CapRateEstimateAdjuster(double Cap) : IRateEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRateEstimateAdjuster(List<double> Bounds, List<double> Rates) : IRateEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateEstimate item, double valueValue)
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

public sealed record MinimumRateEstimateAdjuster(double Minimum) : IRateEstimateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRateEstimateAdjuster(Registry Flags, string Flag, double Percent) : IRateEstimateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate estimate steps in order and keeps a trace of each.</summary>
public sealed class RateEstimateEngine(List<IRateEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateEstimateEngine DefaultRateEstimateEngine(Registry flags)
    {
        return new RateEstimateEngine(
            new List<IRateEstimateModifier>
            {
                new CapRateEstimateAdjuster(40190.0),
                new TieredRateEstimateAdjuster(
                    new List<double> { 128.0, 2757.0, 7265.0 },
                    new List<double> { 1.0, 1.07, 0.83 }),
                new MinimumRateEstimateAdjuster(0.4),
                new FlaggedRateEstimateAdjuster(flags, "rate-estimate-tiered", -5.3),
            },
            new List<string>(),
            2.9,
            1273.3);
    }

    public double Price(RateEstimate item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RateEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
