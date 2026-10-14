namespace Acme.Workforce.Absence.Accrual.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy accrual segment calculation.</summary>
public interface ILegacyAccrualSegmentFactor
{
    string Describe();
    double Adjust(LegacyAccrualSegment item, double valueValue);
}

public sealed record TitleLegacyAccrualSegmentAdjuster(Dictionary<string, double> Factors) : ILegacyAccrualSegmentFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(LegacyAccrualSegment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLegacyAccrualSegmentAdjuster(double Minimum) : ILegacyAccrualSegmentFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyAccrualSegment item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingLegacyAccrualSegmentAdjuster(double Step) : ILegacyAccrualSegmentFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyAccrualSegment item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLegacyAccrualSegmentAdjuster(List<double> Bounds, List<double> Rates) : ILegacyAccrualSegmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyAccrualSegment item, double valueValue)
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

public sealed record FlaggedLegacyAccrualSegmentAdjuster(Registry Flags, string Flag, double Percent) : ILegacyAccrualSegmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyAccrualSegment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LegacyAccrualSegmentScoreOffsetAdjuster(double Weight) : ILegacyAccrualSegmentFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(LegacyAccrualSegment item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the legacy accrual segment steps in order and keeps a trace of each.</summary>
public sealed class LegacyAccrualSegmentEngine(List<ILegacyAccrualSegmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyAccrualSegmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyAccrualSegmentEngine DefaultLegacyAccrualSegmentEngine(Registry flags)
    {
        return new LegacyAccrualSegmentEngine(
            new List<ILegacyAccrualSegmentFactor>
            {
                new TitleLegacyAccrualSegmentAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.17, ["south"] = 1.22 }),
                new MinimumLegacyAccrualSegmentAdjuster(39.4),
                new RoundingLegacyAccrualSegmentAdjuster(0.5),
                new TieredLegacyAccrualSegmentAdjuster(
                    new List<double> { 161.0, 6405.0, 8070.0 },
                    new List<double> { 0.9, 0.94, 0.99 }),
                new FlaggedLegacyAccrualSegmentAdjuster(flags, "legacy-accrual-segment-primary", 6.2),
                new LegacyAccrualSegmentScoreOffsetAdjuster(0.96),
            },
            new List<string>(),
            2.9,
            77809.1);
    }

    public double Evaluate(LegacyAccrualSegment item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyAccrualSegment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
