namespace Acme.Workforce.Absence.Policy.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy rule calculation.</summary>
public interface IPolicyRuleModifier
{
    string Describe();
    double Adjust(PolicyRule item, double valueValue);
}

public sealed record CapPolicyRuleAdjuster(double Cap) : IPolicyRuleModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingPolicyRuleAdjuster(double Step) : IPolicyRuleModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record NamePolicyRuleAdjuster(Dictionary<string, double> Factors) : IPolicyRuleModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyRule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentPolicyRuleAdjuster(double Percent) : IPolicyRuleModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyRule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy rule steps in order and keeps a trace of each.</summary>
public sealed class PolicyRuleEngine(List<IPolicyRuleModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyRuleModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyRuleEngine DefaultPolicyRuleEngine()
    {
        return new PolicyRuleEngine(
            new List<IPolicyRuleModifier>
            {
                new CapPolicyRuleAdjuster(23916.0),
                new RoundingPolicyRuleAdjuster(0.05),
                new NamePolicyRuleAdjuster(new Dictionary<string, double> { ["south"] = 0.97 }),
                new PercentPolicyRuleAdjuster(15.3),
            },
            new List<string>(),
            1.9,
            78192.4);
    }

    public double Run(PolicyRule item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
