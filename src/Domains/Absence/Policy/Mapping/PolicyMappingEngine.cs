namespace Acme.Workforce.Absence.Policy.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy mapping calculation.</summary>
public interface IPolicyMappingStep
{
    string Describe();
    double Adjust(PolicyMapping item, double valueValue);
}

public sealed record PolicyMappingLimitOffsetAdjuster(double Weight) : IPolicyMappingStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PolicyMapping item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record RoundingPolicyMappingAdjuster(double Step) : IPolicyMappingStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record UnitPolicyMappingAdjuster(Dictionary<string, double> Factors) : IPolicyMappingStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PolicyMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPolicyMappingAdjuster(Registry Flags, string Flag, double Percent) : IPolicyMappingStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPolicyMappingAdjuster(double Minimum) : IPolicyMappingStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyMapping item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPolicyMappingAdjuster(List<double> Bounds, List<double> Rates) : IPolicyMappingStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyMapping item, double valueValue)
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

/// <summary>Runs the policy mapping steps in order and keeps a trace of each.</summary>
public sealed class PolicyMappingEngine(List<IPolicyMappingStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyMappingStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyMappingEngine DefaultPolicyMappingEngine(Registry flags)
    {
        return new PolicyMappingEngine(
            new List<IPolicyMappingStep>
            {
                new PolicyMappingLimitOffsetAdjuster(1.07),
                new RoundingPolicyMappingAdjuster(0.25),
                new UnitPolicyMappingAdjuster(new Dictionary<string, double> { ["north"] = 0.92, ["retail"] = 1.2 }),
                new FlaggedPolicyMappingAdjuster(flags, "policy-mapping-default", -7.4),
                new MinimumPolicyMappingAdjuster(40.4),
                new TieredPolicyMappingAdjuster(
                    new List<double> { 170.0, 3822.0, 5050.0, 6197.0 },
                    new List<double> { 1.04, 1.18, 1.03, 0.99 }),
            },
            new List<string>(),
            2.2,
            45635.3);
    }

    public double Price(PolicyMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
