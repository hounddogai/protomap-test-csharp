namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient deduction notice calculation.</summary>
public interface ILenientDeductionNoticeStep
{
    string Describe();
    double Adjust(LenientDeductionNotice item, double valueValue);
}

public sealed record FlaggedLenientDeductionNoticeAdjuster(Registry Flags, string Flag, double Percent) : ILenientDeductionNoticeStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientDeductionNotice item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientDeductionNoticeTenureMonthsOffsetAdjuster(double Weight) : ILenientDeductionNoticeStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(LenientDeductionNotice item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record LabelLenientDeductionNoticeAdjuster(Dictionary<string, double> Factors) : ILenientDeductionNoticeStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientDeductionNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLenientDeductionNoticeAdjuster(double Percent) : ILenientDeductionNoticeStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientDeductionNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumLenientDeductionNoticeAdjuster(double Minimum) : ILenientDeductionNoticeStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientDeductionNotice item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the lenient deduction notice steps in order and keeps a trace of each.</summary>
public sealed class LenientDeductionNoticeEngine(List<ILenientDeductionNoticeStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientDeductionNoticeStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientDeductionNoticeEngine DefaultLenientDeductionNoticeEngine(Registry flags)
    {
        return new LenientDeductionNoticeEngine(
            new List<ILenientDeductionNoticeStep>
            {
                new FlaggedLenientDeductionNoticeAdjuster(flags, "lenient-deduction-notice-legacy", -7.3),
                new LenientDeductionNoticeTenureMonthsOffsetAdjuster(0.36),
                new LabelLenientDeductionNoticeAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.7, ["central"] = 1.26 }),
                new PercentLenientDeductionNoticeAdjuster(-5.3),
                new MinimumLenientDeductionNoticeAdjuster(22.9),
            },
            new List<string>(),
            1.8,
            66057.6);
    }

    public double Price(LenientDeductionNotice item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientDeductionNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
