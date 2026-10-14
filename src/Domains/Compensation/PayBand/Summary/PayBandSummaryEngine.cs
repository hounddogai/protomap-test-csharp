namespace Acme.Workforce.Compensation.PayBand.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay band summary calculation.</summary>
public interface IPayBandSummaryModifier
{
    string Describe();
    double Adjust(PayBandSummary item, double valueValue);
}

public sealed record FlaggedPayBandSummaryAdjuster(Registry Flags, string Flag, double Percent) : IPayBandSummaryModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayBandSummaryAdjuster(double Minimum) : IPayBandSummaryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPayBandSummaryAdjuster(List<double> Bounds, List<double> Rates) : IPayBandSummaryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayBandSummary item, double valueValue)
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

public sealed record CapPayBandSummaryAdjuster(double Cap) : IPayBandSummaryModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandSummary item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the pay band summary steps in order and keeps a trace of each.</summary>
public sealed class PayBandSummaryEngine(List<IPayBandSummaryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandSummaryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandSummaryEngine DefaultPayBandSummaryEngine(Registry flags)
    {
        return new PayBandSummaryEngine(
            new List<IPayBandSummaryModifier>
            {
                new FlaggedPayBandSummaryAdjuster(flags, "pay-band-summary-strict", -0.6),
                new MinimumPayBandSummaryAdjuster(10.4),
                new TieredPayBandSummaryAdjuster(
                    new List<double> { 738.0, 2247.0, 7474.0 },
                    new List<double> { 1.14, 0.91, 0.9 }),
                new CapPayBandSummaryAdjuster(20870.0),
            },
            new List<string>(),
            2.6,
            56908.0);
    }

    public double Evaluate(PayBandSummary item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
