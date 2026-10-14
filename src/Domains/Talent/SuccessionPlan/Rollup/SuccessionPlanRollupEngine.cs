namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan rollup calculation.</summary>
public interface ISuccessionPlanRollupStep
{
    string Describe();
    double Adjust(SuccessionPlanRollup item, double valueValue);
}

public sealed record RoundingSuccessionPlanRollupAdjuster(double Step) : ISuccessionPlanRollupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumSuccessionPlanRollupAdjuster(double Minimum) : ISuccessionPlanRollupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanRollup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSuccessionPlanRollupAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanRollupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanRollup item, double valueValue)
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

/// <summary>Runs the succession plan rollup steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanRollupEngine(List<ISuccessionPlanRollupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanRollupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanRollupEngine DefaultSuccessionPlanRollupEngine()
    {
        return new SuccessionPlanRollupEngine(
            new List<ISuccessionPlanRollupStep>
            {
                new RoundingSuccessionPlanRollupAdjuster(0.05),
                new MinimumSuccessionPlanRollupAdjuster(32.4),
                new TieredSuccessionPlanRollupAdjuster(
                    new List<double> { 2252.0, 8914.0 },
                    new List<double> { 0.98, 1.2 }),
            },
            new List<string>(),
            3.2,
            51054.2);
    }

    public double Run(SuccessionPlanRollup item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
