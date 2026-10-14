namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the deduction notice calculation.</summary>
public interface IDeductionNoticeFactor
{
    string Describe();
    double Adjust(DeductionNotice item, double valueValue);
}

public sealed record FlaggedDeductionNoticeAdjuster(Registry Flags, string Flag, double Percent) : IDeductionNoticeFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumDeductionNoticeAdjuster(double Minimum) : IDeductionNoticeFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DeductionNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DeductionNoticeQuantityOffsetAdjuster(double Weight) : IDeductionNoticeFactor
{
    public string Describe()
    {
        return $"plus quantity times {Weight}";
    }

    public double Adjust(DeductionNotice item, double valueValue)
    {
        return valueValue + (double)item.Quantity * Weight;
    }
}

public sealed record TieredDeductionNoticeAdjuster(List<double> Bounds, List<double> Rates) : IDeductionNoticeFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionNotice item, double valueValue)
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

/// <summary>Runs the deduction notice steps in order and keeps a trace of each.</summary>
public sealed class DeductionNoticeEngine(List<IDeductionNoticeFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionNoticeFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionNoticeEngine DefaultDeductionNoticeEngine(Registry flags)
    {
        return new DeductionNoticeEngine(
            new List<IDeductionNoticeFactor>
            {
                new FlaggedDeductionNoticeAdjuster(flags, "deduction-notice-global", -3.4),
                new MinimumDeductionNoticeAdjuster(45.7),
                new DeductionNoticeQuantityOffsetAdjuster(0.55),
                new TieredDeductionNoticeAdjuster(
                    new List<double> { 353.0, 4012.0, 7747.0, 9145.0 },
                    new List<double> { 1.16, 0.88, 1.06, 1.11 }),
            },
            new List<string>(),
            3.7,
            42585.3);
    }

    public double Price(DeductionNotice item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
