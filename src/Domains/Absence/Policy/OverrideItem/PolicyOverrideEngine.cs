namespace Acme.Workforce.Absence.Policy.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy override calculation.</summary>
public interface IPolicyOverrideStep
{
    string Describe();
    double Adjust(PolicyOverride item, double valueValue);
}

public sealed record CapPolicyOverrideAdjuster(double Cap) : IPolicyOverrideStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyOverrideAdjuster(List<double> Bounds, List<double> Rates) : IPolicyOverrideStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyOverride item, double valueValue)
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

public sealed record PercentPolicyOverrideAdjuster(double Percent) : IPolicyOverrideStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPolicyOverrideAdjuster(double Step) : IPolicyOverrideStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedPolicyOverrideAdjuster(Registry Flags, string Flag, double Percent) : IPolicyOverrideStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy override steps in order and keeps a trace of each.</summary>
public sealed class PolicyOverrideEngine(List<IPolicyOverrideStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyOverrideStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyOverrideEngine DefaultPolicyOverrideEngine(Registry flags)
    {
        return new PolicyOverrideEngine(
            new List<IPolicyOverrideStep>
            {
                new CapPolicyOverrideAdjuster(25532.0),
                new TieredPolicyOverrideAdjuster(
                    new List<double> { 2543.0, 6656.0 },
                    new List<double> { 0.84, 0.87 }),
                new PercentPolicyOverrideAdjuster(-5.5),
                new RoundingPolicyOverrideAdjuster(5.0),
                new FlaggedPolicyOverrideAdjuster(flags, "policy-override-batched", 3.5),
            },
            new List<string>(),
            3.6,
            30497.6);
    }

    public double Compute(PolicyOverride item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
