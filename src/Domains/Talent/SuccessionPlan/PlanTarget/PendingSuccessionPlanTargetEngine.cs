namespace Acme.Workforce.Talent.SuccessionPlan.PlanTarget;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending succession plan target calculation.</summary>
public interface IPendingSuccessionPlanTargetModifier
{
    string Describe();
    double Adjust(PendingSuccessionPlanTarget item, double valueValue);
}

public sealed record PercentPendingSuccessionPlanTargetAdjuster(double Percent) : IPendingSuccessionPlanTargetModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingSuccessionPlanTarget item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelPendingSuccessionPlanTargetAdjuster(Dictionary<string, double> Factors) : IPendingSuccessionPlanTargetModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PendingSuccessionPlanTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPendingSuccessionPlanTargetAdjuster(double Step) : IPendingSuccessionPlanTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingSuccessionPlanTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPendingSuccessionPlanTargetAdjuster(double Minimum) : IPendingSuccessionPlanTargetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingSuccessionPlanTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the pending succession plan target steps in order and keeps a trace of each.</summary>
public sealed class PendingSuccessionPlanTargetEngine(List<IPendingSuccessionPlanTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingSuccessionPlanTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingSuccessionPlanTargetEngine DefaultPendingSuccessionPlanTargetEngine()
    {
        return new PendingSuccessionPlanTargetEngine(
            new List<IPendingSuccessionPlanTargetModifier>
            {
                new PercentPendingSuccessionPlanTargetAdjuster(5.3),
                new LabelPendingSuccessionPlanTargetAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.92, ["west"] = 1.1 }),
                new RoundingPendingSuccessionPlanTargetAdjuster(0.25),
                new MinimumPendingSuccessionPlanTargetAdjuster(48.7),
            },
            new List<string>(),
            2.0,
            19937.9);
    }

    public double Run(PendingSuccessionPlanTarget item)
    {
        var current = item.WeightedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingSuccessionPlanTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
