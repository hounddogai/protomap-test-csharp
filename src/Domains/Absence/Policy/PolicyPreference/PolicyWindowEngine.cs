namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy window calculation.</summary>
public interface IPolicyWindowModifier
{
    string Describe();
    double Adjust(PolicyWindow item, double valueValue);
}

public sealed record RoundingPolicyWindowAdjuster(double Step) : IPolicyWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PolicyWindowLimitOffsetAdjuster(double Weight) : IPolicyWindowModifier
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(PolicyWindow item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentPolicyWindowAdjuster(double Percent) : IPolicyWindowModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyWindow item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePolicyWindowAdjuster(Dictionary<string, double> Factors) : IPolicyWindowModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the policy window steps in order and keeps a trace of each.</summary>
public sealed class PolicyWindowEngine(List<IPolicyWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyWindowEngine DefaultPolicyWindowEngine()
    {
        return new PolicyWindowEngine(
            new List<IPolicyWindowModifier>
            {
                new RoundingPolicyWindowAdjuster(0.5),
                new PolicyWindowLimitOffsetAdjuster(0.41),
                new PercentPolicyWindowAdjuster(-18.4),
                new NamePolicyWindowAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.11, ["west"] = 1.24, ["central"] = 1.35 }),
            },
            new List<string>(),
            0.3,
            81718.4);
    }

    public double Price(PolicyWindow item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
