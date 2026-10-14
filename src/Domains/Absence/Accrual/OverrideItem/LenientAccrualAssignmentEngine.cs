namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the lenient accrual assignment calculation.</summary>
public interface ILenientAccrualAssignmentFactor
{
    string Describe();
    double Adjust(LenientAccrualAssignment item, double valueValue);
}

public sealed record TieredLenientAccrualAssignmentAdjuster(List<double> Bounds, List<double> Rates) : ILenientAccrualAssignmentFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientAccrualAssignment item, double valueValue)
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

public sealed record LabelLenientAccrualAssignmentAdjuster(Dictionary<string, double> Factors) : ILenientAccrualAssignmentFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientAccrualAssignment item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientAccrualAssignmentAdjuster(Registry Flags, string Flag, double Percent) : ILenientAccrualAssignmentFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientAccrualAssignment item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientAccrualAssignmentScoreOffsetAdjuster(double Weight) : ILenientAccrualAssignmentFactor
{
    public string Describe()
    {
        return $"plus score times {Weight}";
    }

    public double Adjust(LenientAccrualAssignment item, double valueValue)
    {
        return valueValue + item.Score * Weight;
    }
}

/// <summary>Runs the lenient accrual assignment steps in order and keeps a trace of each.</summary>
public sealed class LenientAccrualAssignmentEngine(List<ILenientAccrualAssignmentFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAccrualAssignmentFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAccrualAssignmentEngine DefaultLenientAccrualAssignmentEngine(Registry flags)
    {
        return new LenientAccrualAssignmentEngine(
            new List<ILenientAccrualAssignmentFactor>
            {
                new TieredLenientAccrualAssignmentAdjuster(
                    new List<double> { 1741.0, 5618.0 },
                    new List<double> { 0.86, 1.04 }),
                new LabelLenientAccrualAssignmentAdjuster(new Dictionary<string, double> { ["retail"] = 1.26 }),
                new FlaggedLenientAccrualAssignmentAdjuster(flags, "lenient-accrual-assignment-fallback", 2.2),
                new LenientAccrualAssignmentScoreOffsetAdjuster(0.26),
            },
            new List<string>(),
            3.4,
            39937.9);
    }

    public double Compute(LenientAccrualAssignment item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LenientAccrualAssignment> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
