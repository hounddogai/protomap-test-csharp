namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan request calculation.</summary>
public interface ISuccessionPlanRequestStep
{
    string Describe();
    double Adjust(SuccessionPlanRequest item, double valueValue);
}

public sealed record MinimumSuccessionPlanRequestAdjuster(double Minimum) : ISuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSuccessionPlanRequestAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanRequest item, double valueValue)
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

public sealed record SuccessionPlanRequestMinimumOffsetAdjuster(double Weight) : ISuccessionPlanRequestStep
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(SuccessionPlanRequest item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

/// <summary>Runs the succession plan request steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanRequestEngine(List<ISuccessionPlanRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanRequestEngine DefaultSuccessionPlanRequestEngine()
    {
        return new SuccessionPlanRequestEngine(
            new List<ISuccessionPlanRequestStep>
            {
                new MinimumSuccessionPlanRequestAdjuster(29.3),
                new TieredSuccessionPlanRequestAdjuster(
                    new List<double> { 76.0, 396.0, 9702.0 },
                    new List<double> { 0.93, 0.89, 0.9 }),
                new SuccessionPlanRequestMinimumOffsetAdjuster(0.11),
            },
            new List<string>(),
            0.3,
            82538.0);
    }

    public double Compute(SuccessionPlanRequest item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
