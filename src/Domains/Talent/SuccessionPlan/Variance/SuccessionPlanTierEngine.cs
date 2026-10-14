namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan tier calculation.</summary>
public interface ISuccessionPlanTierStep
{
    string Describe();
    double Adjust(SuccessionPlanTier item, double valueValue);
}

public sealed record TieredSuccessionPlanTierAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanTier item, double valueValue)
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

public sealed record RoundingSuccessionPlanTierAdjuster(double Step) : ISuccessionPlanTierStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSuccessionPlanTierAdjuster(double Minimum) : ISuccessionPlanTierStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the succession plan tier steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanTierEngine(List<ISuccessionPlanTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanTierEngine DefaultSuccessionPlanTierEngine()
    {
        return new SuccessionPlanTierEngine(
            new List<ISuccessionPlanTierStep>
            {
                new TieredSuccessionPlanTierAdjuster(
                    new List<double> { 5801.0, 6321.0, 8562.0, 9839.0 },
                    new List<double> { 0.93, 0.83, 1.03, 1.04 }),
                new RoundingSuccessionPlanTierAdjuster(1.0),
                new MinimumSuccessionPlanTierAdjuster(9.3),
            },
            new List<string>(),
            0.6,
            44082.9);
    }

    public double Compute(SuccessionPlanTier item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
