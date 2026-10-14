namespace Acme.Workforce.Payrollops.Deduction.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional deduction batch calculation.</summary>
public interface IRegionalDeductionBatchStep
{
    string Describe();
    double Adjust(RegionalDeductionBatch item, double valueValue);
}

public sealed record RoundingRegionalDeductionBatchAdjuster(double Step) : IRegionalDeductionBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalDeductionBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalDeductionBatchAdjuster(double Percent) : IRegionalDeductionBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalDeductionBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionalDeductionBatchCostOffsetAdjuster(double Weight) : IRegionalDeductionBatchStep
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(RegionalDeductionBatch item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record FlaggedRegionalDeductionBatchAdjuster(Registry Flags, string Flag, double Percent) : IRegionalDeductionBatchStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalDeductionBatch item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional deduction batch steps in order and keeps a trace of each.</summary>
public sealed class RegionalDeductionBatchEngine(List<IRegionalDeductionBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalDeductionBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalDeductionBatchEngine DefaultRegionalDeductionBatchEngine(Registry flags)
    {
        return new RegionalDeductionBatchEngine(
            new List<IRegionalDeductionBatchStep>
            {
                new RoundingRegionalDeductionBatchAdjuster(0.5),
                new PercentRegionalDeductionBatchAdjuster(-16.0),
                new RegionalDeductionBatchCostOffsetAdjuster(1.34),
                new FlaggedRegionalDeductionBatchAdjuster(flags, "regional-deduction-batch-rolling", 2.1),
            },
            new List<string>(),
            4.6,
            78461.7);
    }

    public double Evaluate(RegionalDeductionBatch item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalDeductionBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
