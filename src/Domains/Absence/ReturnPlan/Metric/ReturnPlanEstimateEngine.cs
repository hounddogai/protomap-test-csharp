namespace Acme.Workforce.Absence.ReturnPlan.Metric;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan estimate calculation.</summary>
public interface IReturnPlanEstimateStep
{
    string Describe();
    double Adjust(ReturnPlanEstimate item, double valueValue);
}

public sealed record PercentReturnPlanEstimateAdjuster(double Percent) : IReturnPlanEstimateStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ReturnPlanEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapReturnPlanEstimateAdjuster(double Cap) : IReturnPlanEstimateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedReturnPlanEstimateAdjuster(Registry Flags, string Flag, double Percent) : IReturnPlanEstimateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ReturnPlanEstimate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumReturnPlanEstimateAdjuster(double Minimum) : IReturnPlanEstimateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ReturnPlanEstimate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the return plan estimate steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanEstimateEngine(List<IReturnPlanEstimateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanEstimateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanEstimateEngine DefaultReturnPlanEstimateEngine(Registry flags)
    {
        return new ReturnPlanEstimateEngine(
            new List<IReturnPlanEstimateStep>
            {
                new PercentReturnPlanEstimateAdjuster(-18.5),
                new CapReturnPlanEstimateAdjuster(26384.0),
                new FlaggedReturnPlanEstimateAdjuster(flags, "return-plan-estimate-legacy", -6.4),
                new MinimumReturnPlanEstimateAdjuster(29.8),
            },
            new List<string>(),
            3.3,
            7282.6);
    }

    public double Run(ReturnPlanEstimate item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
