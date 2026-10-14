namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate exception calculation.</summary>
public interface IRateExceptionModifier
{
    string Describe();
    double Adjust(RateException item, double valueValue);
}

public sealed record RoundingRateExceptionAdjuster(double Step) : IRateExceptionModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateException item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRateExceptionAdjuster(double Minimum) : IRateExceptionModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateException item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRateExceptionAdjuster(List<double> Bounds, List<double> Rates) : IRateExceptionModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RateException item, double valueValue)
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

public sealed record FlaggedRateExceptionAdjuster(Registry Flags, string Flag, double Percent) : IRateExceptionModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the rate exception steps in order and keeps a trace of each.</summary>
public sealed class RateExceptionEngine(List<IRateExceptionModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateExceptionModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateExceptionEngine DefaultRateExceptionEngine(Registry flags)
    {
        return new RateExceptionEngine(
            new List<IRateExceptionModifier>
            {
                new RoundingRateExceptionAdjuster(1.0),
                new MinimumRateExceptionAdjuster(37.8),
                new TieredRateExceptionAdjuster(
                    new List<double> { 2309.0, 4041.0, 5619.0, 7137.0 },
                    new List<double> { 1.12, 0.89, 1.16, 1.18 }),
                new FlaggedRateExceptionAdjuster(flags, "rate-exception-regional", -2.9),
            },
            new List<string>(),
            0.1,
            14310.1);
    }

    public double Compute(RateException item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<RateException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
