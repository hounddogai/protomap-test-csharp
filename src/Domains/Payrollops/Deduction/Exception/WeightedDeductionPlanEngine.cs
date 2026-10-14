namespace Acme.Workforce.Payrollops.Deduction.Exception;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted deduction plan calculation.</summary>
public interface IWeightedDeductionPlanAdjuster
{
    string Describe();
    double Adjust(WeightedDeductionPlan item, double valueValue);
}

public sealed record PercentWeightedDeductionPlanAdjuster(double Percent) : IWeightedDeductionPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedDeductionPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LocaleWeightedDeductionPlanAdjuster(Dictionary<string, double> Factors) : IWeightedDeductionPlanAdjuster
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(WeightedDeductionPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingWeightedDeductionPlanAdjuster(double Step) : IWeightedDeductionPlanAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedDeductionPlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedWeightedDeductionPlanAdjuster(Registry Flags, string Flag, double Percent) : IWeightedDeductionPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedDeductionPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted deduction plan steps in order and keeps a trace of each.</summary>
public sealed class WeightedDeductionPlanEngine(List<IWeightedDeductionPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedDeductionPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedDeductionPlanEngine DefaultWeightedDeductionPlanEngine(Registry flags)
    {
        return new WeightedDeductionPlanEngine(
            new List<IWeightedDeductionPlanAdjuster>
            {
                new PercentWeightedDeductionPlanAdjuster(3.6),
                new LocaleWeightedDeductionPlanAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.03, ["east"] = 1.06, ["north"] = 0.85 }),
                new RoundingWeightedDeductionPlanAdjuster(5.0),
                new FlaggedWeightedDeductionPlanAdjuster(flags, "weighted-deduction-plan-weighted", -1.9),
            },
            new List<string>(),
            1.9,
            13319.6);
    }

    public double Evaluate(WeightedDeductionPlan item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedDeductionPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
