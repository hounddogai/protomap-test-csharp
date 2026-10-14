namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay run variance calculation.</summary>
public interface IPayRunVarianceModifier
{
    string Describe();
    double Adjust(PayRunVariance item, double valueValue);
}

public sealed record MinimumPayRunVarianceAdjuster(double Minimum) : IPayRunVarianceModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayRunVariance item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPayRunVarianceAdjuster(List<double> Bounds, List<double> Rates) : IPayRunVarianceModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PayRunVariance item, double valueValue)
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

public sealed record PercentPayRunVarianceAdjuster(double Percent) : IPayRunVarianceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayRunVariance item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPayRunVarianceAdjuster(Registry Flags, string Flag, double Percent) : IPayRunVarianceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayRunVariance item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay run variance steps in order and keeps a trace of each.</summary>
public sealed class PayRunVarianceEngine(List<IPayRunVarianceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayRunVarianceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayRunVarianceEngine DefaultPayRunVarianceEngine(Registry flags)
    {
        return new PayRunVarianceEngine(
            new List<IPayRunVarianceModifier>
            {
                new MinimumPayRunVarianceAdjuster(34.6),
                new TieredPayRunVarianceAdjuster(
                    new List<double> { 5144.0, 6741.0, 9353.0 },
                    new List<double> { 1.06, 0.99, 1.14 }),
                new PercentPayRunVarianceAdjuster(13.8),
                new FlaggedPayRunVarianceAdjuster(flags, "pay-run-variance-scheduled", -1.1),
            },
            new List<string>(),
            1.5,
            40684.0);
    }

    public double Run(PayRunVariance item)
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

    public Dictionary<string, double> EvaluateAll(List<PayRunVariance> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
