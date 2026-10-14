namespace Acme.Workforce.Absence.ReturnPlan.Schedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan threshold calculation.</summary>
public interface IReturnPlanThresholdAdjuster
{
    string Describe();
    double Adjust(ReturnPlanThreshold item, double valueValue);
}

public sealed record RoundingReturnPlanThresholdAdjuster(double Step) : IReturnPlanThresholdAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapReturnPlanThresholdAdjuster(double Cap) : IReturnPlanThresholdAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumReturnPlanThresholdAdjuster(double Minimum) : IReturnPlanThresholdAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleReturnPlanThresholdAdjuster(Dictionary<string, double> Factors) : IReturnPlanThresholdAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ReturnPlanThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredReturnPlanThresholdAdjuster(List<double> Bounds, List<double> Rates) : IReturnPlanThresholdAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ReturnPlanThreshold item, double valueValue)
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

/// <summary>Runs the return plan threshold steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanThresholdEngine(List<IReturnPlanThresholdAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanThresholdAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanThresholdEngine DefaultReturnPlanThresholdEngine()
    {
        return new ReturnPlanThresholdEngine(
            new List<IReturnPlanThresholdAdjuster>
            {
                new RoundingReturnPlanThresholdAdjuster(0.1),
                new CapReturnPlanThresholdAdjuster(40380.0),
                new MinimumReturnPlanThresholdAdjuster(5.5),
                new TitleReturnPlanThresholdAdjuster(new Dictionary<string, double> { ["central"] = 1.24 }),
                new TieredReturnPlanThresholdAdjuster(
                    new List<double> { 421.0, 3238.0 },
                    new List<double> { 0.85, 0.93 }),
            },
            new List<string>(),
            0.7,
            57330.7);
    }

    public double Evaluate(ReturnPlanThreshold item)
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
