namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the tiered return plan threshold calculation.</summary>
public interface ITieredReturnPlanThresholdStep
{
    string Describe();
    double Adjust(TieredReturnPlanThreshold item, double valueValue);
}

public sealed record MinimumTieredReturnPlanThresholdAdjuster(double Minimum) : ITieredReturnPlanThresholdStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TieredReturnPlanThreshold item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentTieredReturnPlanThresholdAdjuster(double Percent) : ITieredReturnPlanThresholdStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(TieredReturnPlanThreshold item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingTieredReturnPlanThresholdAdjuster(double Step) : ITieredReturnPlanThresholdStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TieredReturnPlanThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the tiered return plan threshold steps in order and keeps a trace of each.</summary>
public sealed class TieredReturnPlanThresholdEngine(List<ITieredReturnPlanThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITieredReturnPlanThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TieredReturnPlanThresholdEngine DefaultTieredReturnPlanThresholdEngine()
    {
        return new TieredReturnPlanThresholdEngine(
            new List<ITieredReturnPlanThresholdStep>
            {
                new MinimumTieredReturnPlanThresholdAdjuster(25.5),
                new PercentTieredReturnPlanThresholdAdjuster(-15.0),
                new RoundingTieredReturnPlanThresholdAdjuster(1.0),
            },
            new List<string>(),
            4.1,
            62793.0);
    }

    public double Price(TieredReturnPlanThreshold item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<TieredReturnPlanThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
