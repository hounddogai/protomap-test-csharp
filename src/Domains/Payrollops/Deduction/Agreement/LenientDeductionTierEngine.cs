namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient deduction tier calculation.</summary>
public interface ILenientDeductionTierStep
{
    string Describe();
    double Adjust(LenientDeductionTier item, double valueValue);
}

public sealed record NameLenientDeductionTierAdjuster(Dictionary<string, double> Factors) : ILenientDeductionTierStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientDeductionTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record LenientDeductionTierMarginOffsetAdjuster(double Weight) : ILenientDeductionTierStep
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(LenientDeductionTier item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record RoundingLenientDeductionTierAdjuster(double Step) : ILenientDeductionTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientDeductionTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLenientDeductionTierAdjuster(double Cap) : ILenientDeductionTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientDeductionTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient deduction tier steps in order and keeps a trace of each.</summary>
public sealed class LenientDeductionTierEngine(List<ILenientDeductionTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientDeductionTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientDeductionTierEngine DefaultLenientDeductionTierEngine()
    {
        return new LenientDeductionTierEngine(
            new List<ILenientDeductionTierStep>
            {
                new NameLenientDeductionTierAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.25, ["north"] = 1.31 }),
                new LenientDeductionTierMarginOffsetAdjuster(0.06),
                new RoundingLenientDeductionTierAdjuster(1.0),
                new CapLenientDeductionTierAdjuster(30702.0),
            },
            new List<string>(),
            1.7,
            80059.3);
    }

    public double Evaluate(LenientDeductionTier item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientDeductionTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
