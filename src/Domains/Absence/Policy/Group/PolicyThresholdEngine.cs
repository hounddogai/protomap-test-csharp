namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy threshold calculation.</summary>
public interface IPolicyThresholdStep
{
    string Describe();
    double Adjust(PolicyThreshold item, double valueValue);
}

public sealed record RoundingPolicyThresholdAdjuster(double Step) : IPolicyThresholdStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyThreshold item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitPolicyThresholdAdjuster(Dictionary<string, double> Factors) : IPolicyThresholdStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PolicyThreshold item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyThresholdAdjuster(double Cap) : IPolicyThresholdStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyThreshold item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy threshold steps in order and keeps a trace of each.</summary>
public sealed class PolicyThresholdEngine(List<IPolicyThresholdStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyThresholdStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyThresholdEngine DefaultPolicyThresholdEngine()
    {
        return new PolicyThresholdEngine(
            new List<IPolicyThresholdStep>
            {
                new RoundingPolicyThresholdAdjuster(0.05),
                new UnitPolicyThresholdAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.79, ["central"] = 0.97 }),
                new CapPolicyThresholdAdjuster(45202.0),
            },
            new List<string>(),
            0.2,
            10816.5);
    }

    public double Compute(PolicyThreshold item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyThreshold> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
