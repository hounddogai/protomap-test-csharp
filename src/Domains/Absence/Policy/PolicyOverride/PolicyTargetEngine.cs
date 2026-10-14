namespace Acme.Workforce.Absence.Policy.PolicyOverride;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy target calculation.</summary>
public interface IPolicyTargetModifier
{
    string Describe();
    double Adjust(PolicyTarget item, double valueValue);
}

public sealed record TieredPolicyTargetAdjuster(List<double> Bounds, List<double> Rates) : IPolicyTargetModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyTarget item, double valueValue)
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

public sealed record CategoryPolicyTargetAdjuster(Dictionary<string, double> Factors) : IPolicyTargetModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PolicyTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapPolicyTargetAdjuster(double Cap) : IPolicyTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PolicyTargetBudgetOffsetAdjuster(double Weight) : IPolicyTargetModifier
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PolicyTarget item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record RoundingPolicyTargetAdjuster(double Step) : IPolicyTargetModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the policy target steps in order and keeps a trace of each.</summary>
public sealed class PolicyTargetEngine(List<IPolicyTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyTargetEngine DefaultPolicyTargetEngine()
    {
        return new PolicyTargetEngine(
            new List<IPolicyTargetModifier>
            {
                new TieredPolicyTargetAdjuster(
                    new List<double> { 2679.0, 4048.0, 6779.0, 9968.0 },
                    new List<double> { 1.1, 0.99, 1.16, 1.11 }),
                new CategoryPolicyTargetAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.97, ["online"] = 0.99, ["south"] = 0.94 }),
                new CapPolicyTargetAdjuster(28927.0),
                new PolicyTargetBudgetOffsetAdjuster(0.14),
                new RoundingPolicyTargetAdjuster(0.25),
            },
            new List<string>(),
            0.8,
            8945.4);
    }

    public double Evaluate(PolicyTarget item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
