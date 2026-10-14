namespace Acme.Workforce.Absence.ReturnPlan.PlanAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted return plan override calculation.</summary>
public interface IWeightedReturnPlanOverrideModifier
{
    string Describe();
    double Adjust(WeightedReturnPlanOverride item, double valueValue);
}

public sealed record MinimumWeightedReturnPlanOverrideAdjuster(double Minimum) : IWeightedReturnPlanOverrideModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedReturnPlanOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentWeightedReturnPlanOverrideAdjuster(double Percent) : IWeightedReturnPlanOverrideModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedReturnPlanOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWeightedReturnPlanOverrideAdjuster(double Step) : IWeightedReturnPlanOverrideModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedReturnPlanOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the weighted return plan override steps in order and keeps a trace of each.</summary>
public sealed class WeightedReturnPlanOverrideEngine(List<IWeightedReturnPlanOverrideModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedReturnPlanOverrideModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedReturnPlanOverrideEngine DefaultWeightedReturnPlanOverrideEngine()
    {
        return new WeightedReturnPlanOverrideEngine(
            new List<IWeightedReturnPlanOverrideModifier>
            {
                new MinimumWeightedReturnPlanOverrideAdjuster(43.5),
                new PercentWeightedReturnPlanOverrideAdjuster(-20.4),
                new RoundingWeightedReturnPlanOverrideAdjuster(0.1),
            },
            new List<string>(),
            0.7,
            21410.5);
    }

    public double Price(WeightedReturnPlanOverride item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedReturnPlanOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
