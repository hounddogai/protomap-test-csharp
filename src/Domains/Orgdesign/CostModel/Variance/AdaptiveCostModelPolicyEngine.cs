namespace Acme.Workforce.Orgdesign.CostModel.Variance;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive cost model policy calculation.</summary>
public interface IAdaptiveCostModelPolicyAdjuster
{
    string Describe();
    double Adjust(AdaptiveCostModelPolicy item, double valueValue);
}

public sealed record MinimumAdaptiveCostModelPolicyAdjuster(double Minimum) : IAdaptiveCostModelPolicyAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveCostModelPolicy item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingAdaptiveCostModelPolicyAdjuster(double Step) : IAdaptiveCostModelPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveCostModelPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapAdaptiveCostModelPolicyAdjuster(double Cap) : IAdaptiveCostModelPolicyAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveCostModelPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the adaptive cost model policy steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveCostModelPolicyEngine(List<IAdaptiveCostModelPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveCostModelPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveCostModelPolicyEngine DefaultAdaptiveCostModelPolicyEngine()
    {
        return new AdaptiveCostModelPolicyEngine(
            new List<IAdaptiveCostModelPolicyAdjuster>
            {
                new MinimumAdaptiveCostModelPolicyAdjuster(11.1),
                new RoundingAdaptiveCostModelPolicyAdjuster(0.1),
                new CapAdaptiveCostModelPolicyAdjuster(37279.0),
            },
            new List<string>(),
            2.4,
            94354.2);
    }

    public double Run(AdaptiveCostModelPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveCostModelPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
