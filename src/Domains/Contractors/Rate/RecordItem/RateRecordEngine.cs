namespace Acme.Workforce.Contractors.Rate.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate record calculation.</summary>
public interface IRateRecordStep
{
    string Describe();
    double Adjust(RateRecord item, double valueValue);
}

public sealed record TieredRateRecordAdjuster(List<double> Bounds, List<double> Rates) : IRateRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateRecord item, double valueValue)
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

public sealed record PercentRateRecordAdjuster(double Percent) : IRateRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RateRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRateRecordAdjuster(Registry Flags, string Flag, double Percent) : IRateRecordStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRateRecordAdjuster(double Step) : IRateRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRateRecordAdjuster(double Cap) : IRateRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumRateRecordAdjuster(double Minimum) : IRateRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rate record steps in order and keeps a trace of each.</summary>
public sealed class RateRecordEngine(List<IRateRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateRecordEngine DefaultRateRecordEngine(Registry flags)
    {
        return new RateRecordEngine(
            new List<IRateRecordStep>
            {
                new TieredRateRecordAdjuster(
                    new List<double> { 1412.0, 7988.0, 9096.0 },
                    new List<double> { 1.06, 1.11, 0.92 }),
                new PercentRateRecordAdjuster(9.8),
                new FlaggedRateRecordAdjuster(flags, "rate-record-rolling", 8.1),
                new RoundingRateRecordAdjuster(0.05),
                new CapRateRecordAdjuster(37833.0),
                new MinimumRateRecordAdjuster(37.4),
            },
            new List<string>(),
            0.8,
            55590.4);
    }

    public double Compute(RateRecord item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RateRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
