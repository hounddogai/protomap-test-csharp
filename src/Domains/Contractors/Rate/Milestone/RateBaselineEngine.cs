namespace Acme.Workforce.Contractors.Rate.Milestone;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rate baseline calculation.</summary>
public interface IRateBaselineFactor
{
    string Describe();
    double Adjust(RateBaseline item, double valueValue);
}

public sealed record CapRateBaselineAdjuster(double Cap) : IRateBaselineFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRateBaselineAdjuster(List<double> Bounds, List<double> Rates) : IRateBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateBaseline item, double valueValue)
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

public sealed record PercentRateBaselineAdjuster(double Percent) : IRateBaselineFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateBaseline item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRateBaselineAdjuster(Registry Flags, string Flag, double Percent) : IRateBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RateBaselineSequenceOffsetAdjuster(double Weight) : IRateBaselineFactor
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(RateBaseline item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record MinimumRateBaselineAdjuster(double Minimum) : IRateBaselineFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateBaseline item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rate baseline steps in order and keeps a trace of each.</summary>
public sealed class RateBaselineEngine(List<IRateBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateBaselineEngine DefaultRateBaselineEngine(Registry flags)
    {
        return new RateBaselineEngine(
            new List<IRateBaselineFactor>
            {
                new CapRateBaselineAdjuster(9252.0),
                new TieredRateBaselineAdjuster(
                    new List<double> { 832.0, 3324.0, 6299.0 },
                    new List<double> { 1.08, 0.89, 1.16 }),
                new PercentRateBaselineAdjuster(-10.3),
                new FlaggedRateBaselineAdjuster(flags, "rate-baseline-capped", 2.0),
                new RateBaselineSequenceOffsetAdjuster(0.79),
                new MinimumRateBaselineAdjuster(32.2),
            },
            new List<string>(),
            0.5,
            93841.5);
    }

    public double Price(RateBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<RateBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
