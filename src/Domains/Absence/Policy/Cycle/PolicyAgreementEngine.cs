namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy agreement calculation.</summary>
public interface IPolicyAgreementModifier
{
    string Describe();
    double Adjust(PolicyAgreement item, double valueValue);
}

public sealed record MinimumPolicyAgreementAdjuster(double Minimum) : IPolicyAgreementModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedPolicyAgreementAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAgreementModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAgreement item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelPolicyAgreementAdjuster(Dictionary<string, double> Factors) : IPolicyAgreementModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(PolicyAgreement item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the policy agreement steps in order and keeps a trace of each.</summary>
public sealed class PolicyAgreementEngine(List<IPolicyAgreementModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAgreementModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAgreementEngine DefaultPolicyAgreementEngine(Registry flags)
    {
        return new PolicyAgreementEngine(
            new List<IPolicyAgreementModifier>
            {
                new MinimumPolicyAgreementAdjuster(6.6),
                new FlaggedPolicyAgreementAdjuster(flags, "policy-agreement-adaptive", 7.1),
                new LabelPolicyAgreementAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.96, ["south"] = 1.39 }),
            },
            new List<string>(),
            4.8,
            13290.9);
    }

    public double Evaluate(PolicyAgreement item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
