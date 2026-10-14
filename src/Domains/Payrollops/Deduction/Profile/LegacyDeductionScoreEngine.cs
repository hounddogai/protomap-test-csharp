namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy deduction score calculation.</summary>
public interface ILegacyDeductionScoreStep
{
    string Describe();
    double Adjust(LegacyDeductionScore item, double valueValue);
}

public sealed record PercentLegacyDeductionScoreAdjuster(double Percent) : ILegacyDeductionScoreStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyDeductionScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyDeductionScoreAdjuster(double Cap) : ILegacyDeductionScoreStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyDeductionScore item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingLegacyDeductionScoreAdjuster(double Step) : ILegacyDeductionScoreStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyDeductionScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the legacy deduction score steps in order and keeps a trace of each.</summary>
public sealed class LegacyDeductionScoreEngine(List<ILegacyDeductionScoreStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyDeductionScoreStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyDeductionScoreEngine DefaultLegacyDeductionScoreEngine()
    {
        return new LegacyDeductionScoreEngine(
            new List<ILegacyDeductionScoreStep>
            {
                new PercentLegacyDeductionScoreAdjuster(14.2),
                new CapLegacyDeductionScoreAdjuster(13568.0),
                new RoundingLegacyDeductionScoreAdjuster(0.1),
            },
            new List<string>(),
            1.9,
            35444.4);
    }

    public double Compute(LegacyDeductionScore item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyDeductionScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
