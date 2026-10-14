namespace Acme.Workforce.Absence.Policy.Profile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy profile calculation.</summary>
public interface IPolicyProfileModifier
{
    string Describe();
    double Adjust(PolicyProfile item, double valueValue);
}

public sealed record MinimumPolicyProfileAdjuster(double Minimum) : IPolicyProfileModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyProfile item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPolicyProfileAdjuster(double Cap) : IPolicyProfileModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyProfile item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CategoryPolicyProfileAdjuster(Dictionary<string, double> Factors) : IPolicyProfileModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PolicyProfile item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the policy profile steps in order and keeps a trace of each.</summary>
public sealed class PolicyProfileEngine(List<IPolicyProfileModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyProfileModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyProfileEngine DefaultPolicyProfileEngine()
    {
        return new PolicyProfileEngine(
            new List<IPolicyProfileModifier>
            {
                new MinimumPolicyProfileAdjuster(28.3),
                new CapPolicyProfileAdjuster(47749.0),
                new CategoryPolicyProfileAdjuster(new Dictionary<string, double> { ["retail"] = 0.71 }),
            },
            new List<string>(),
            1.3,
            54109.8);
    }

    public double Price(PolicyProfile item)
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

    public Dictionary<string, double> EvaluateAll(List<PolicyProfile> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
