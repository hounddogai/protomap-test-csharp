namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction plan calculation.</summary>
public interface IDeductionPlanStep
{
    string Describe();
    double Adjust(DeductionPlan item, double valueValue);
}

public sealed record TieredDeductionPlanAdjuster(List<double> Bounds, List<double> Rates) : IDeductionPlanStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeductionPlan item, double valueValue)
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

public sealed record PercentDeductionPlanAdjuster(double Percent) : IDeductionPlanStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDeductionPlanAdjuster(double Step) : IDeductionPlanStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record DeductionPlanStartsAtOffsetAdjuster(double Weight) : IDeductionPlanStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(DeductionPlan item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record SegmentDeductionPlanAdjuster(Dictionary<string, double> Factors) : IDeductionPlanStep
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(DeductionPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the deduction plan steps in order and keeps a trace of each.</summary>
public sealed class DeductionPlanEngine(List<IDeductionPlanStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionPlanStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionPlanEngine DefaultDeductionPlanEngine()
    {
        return new DeductionPlanEngine(
            new List<IDeductionPlanStep>
            {
                new TieredDeductionPlanAdjuster(
                    new List<double> { 1152.0, 3016.0, 9546.0 },
                    new List<double> { 1.12, 0.92, 0.89 }),
                new PercentDeductionPlanAdjuster(-14.8),
                new RoundingDeductionPlanAdjuster(1.0),
                new DeductionPlanStartsAtOffsetAdjuster(0.13),
                new SegmentDeductionPlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.06, ["retail"] = 1.27, ["north"] = 0.81 }),
            },
            new List<string>(),
            4.5,
            4161.5);
    }

    public double Evaluate(DeductionPlan item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
