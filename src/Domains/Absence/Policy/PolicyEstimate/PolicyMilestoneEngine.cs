namespace Acme.Workforce.Absence.Policy.PolicyEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy milestone calculation.</summary>
public interface IPolicyMilestoneModifier
{
    string Describe();
    double Adjust(PolicyMilestone item, double valueValue);
}

public sealed record PercentPolicyMilestoneAdjuster(double Percent) : IPolicyMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPolicyMilestoneAdjuster(double Step) : IPolicyMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredPolicyMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IPolicyMilestoneModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyMilestone item, double valueValue)
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

public sealed record CapPolicyMilestoneAdjuster(double Cap) : IPolicyMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the policy milestone steps in order and keeps a trace of each.</summary>
public sealed class PolicyMilestoneEngine(List<IPolicyMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyMilestoneEngine DefaultPolicyMilestoneEngine()
    {
        return new PolicyMilestoneEngine(
            new List<IPolicyMilestoneModifier>
            {
                new PercentPolicyMilestoneAdjuster(-0.2),
                new RoundingPolicyMilestoneAdjuster(0.5),
                new TieredPolicyMilestoneAdjuster(
                    new List<double> { 190.0, 5259.0, 6991.0 },
                    new List<double> { 0.85, 0.87, 1.0 }),
                new CapPolicyMilestoneAdjuster(43996.0),
            },
            new List<string>(),
            1.4,
            88052.1);
    }

    public double Price(PolicyMilestone item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
