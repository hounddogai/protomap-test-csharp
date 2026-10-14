namespace Acme.Workforce.Absence.ReturnPlan.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the return plan target calculation.</summary>
public interface IReturnPlanTargetStep
{
    string Describe();
    double Adjust(ReturnPlanTarget item, double valueValue);
}

public sealed record FlaggedReturnPlanTargetAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanTargetStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredReturnPlanTargetAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanTargetStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanTarget item, double valueValue)
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

public sealed record CountryReturnPlanTargetAdjuster(Dictionary<string, double> Factors) : IReturnPlanTargetStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ReturnPlanTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the return plan target steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanTargetEngine(List<IReturnPlanTargetStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanTargetStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanTargetEngine DefaultReturnPlanTargetEngine(Registry flags)
    {
        return new ReturnPlanTargetEngine(
            new List<IReturnPlanTargetStep>
            {
                new FlaggedReturnPlanTargetAdjuster(flags, "return-plan-target-manual", -6.3),
                new TieredReturnPlanTargetAdjuster(
                    new List<double> { 3188.0, 5580.0, 8128.0 },
                    new List<double> { 1.12, 0.83, 0.9 }),
                new CountryReturnPlanTargetAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.97, ["online"] = 1.09, ["north"] = 0.79 }),
            },
            new List<string>(),
            2.4,
            65320.4);
    }

    public double Compute(ReturnPlanTarget item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
