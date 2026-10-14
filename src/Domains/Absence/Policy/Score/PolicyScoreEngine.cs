namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy score calculation.</summary>
public interface IPolicyScoreModifier
{
    string Describe();
    double Adjust(PolicyScore item, double valueValue);
}

public sealed record FlaggedPolicyScoreAdjuster(Registry Flags, string Flag, double Percent) : IPolicyScoreModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyScore item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPolicyScoreAdjuster(double Step) : IPolicyScoreModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyScore item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentPolicyScoreAdjuster(double Percent) : IPolicyScoreModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyScore item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the policy score steps in order and keeps a trace of each.</summary>
public sealed class PolicyScoreEngine(List<IPolicyScoreModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyScoreModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyScoreEngine DefaultPolicyScoreEngine(Registry flags)
    {
        return new PolicyScoreEngine(
            new List<IPolicyScoreModifier>
            {
                new FlaggedPolicyScoreAdjuster(flags, "policy-score-default", 7.0),
                new RoundingPolicyScoreAdjuster(1.0),
                new PercentPolicyScoreAdjuster(-10.1),
            },
            new List<string>(),
            3.6,
            36631.0);
    }

    public double Evaluate(PolicyScore item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyScore> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
