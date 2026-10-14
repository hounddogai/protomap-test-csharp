namespace Acme.Workforce.Absence.Policy.Assignment;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy entry calculation.</summary>
public interface IPolicyEntryModifier
{
    string Describe();
    double Adjust(PolicyEntry item, double valueValue);
}

public sealed record TieredPolicyEntryAdjuster(List<double> Bounds, List<double> Rates) : IPolicyEntryModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyEntry item, double valueValue)
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

public sealed record NamePolicyEntryAdjuster(Dictionary<string, double> Factors) : IPolicyEntryModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyEntry item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingPolicyEntryAdjuster(double Step) : IPolicyEntryModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyEntry item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPolicyEntryAdjuster(double Minimum) : IPolicyEntryModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyEntry item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the policy entry steps in order and keeps a trace of each.</summary>
public sealed class PolicyEntryEngine(List<IPolicyEntryModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyEntryModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyEntryEngine DefaultPolicyEntryEngine()
    {
        return new PolicyEntryEngine(
            new List<IPolicyEntryModifier>
            {
                new TieredPolicyEntryAdjuster(
                    new List<double> { 4164.0, 5004.0, 5228.0 },
                    new List<double> { 1.18, 1.14, 0.9 }),
                new NamePolicyEntryAdjuster(new Dictionary<string, double> { ["retail"] = 0.8 }),
                new RoundingPolicyEntryAdjuster(1.0),
                new MinimumPolicyEntryAdjuster(38.7),
            },
            new List<string>(),
            4.7,
            16346.4);
    }

    public double Run(PolicyEntry item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyEntry> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
