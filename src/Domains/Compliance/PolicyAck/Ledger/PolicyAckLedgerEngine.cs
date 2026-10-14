namespace Acme.Workforce.Compliance.PolicyAck.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ack ledger calculation.</summary>
public interface IPolicyAckLedgerStep
{
    string Describe();
    double Adjust(PolicyAckLedger item, double valueValue);
}

public sealed record FlaggedPolicyAckLedgerAdjuster(Registry Flags, string Flag, double Percent) : IPolicyAckLedgerStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyAckLedger item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPolicyAckLedgerAdjuster(double Percent) : IPolicyAckLedgerStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyAckLedger item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPolicyAckLedgerAdjuster(double Step) : IPolicyAckLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyAckLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumPolicyAckLedgerAdjuster(double Minimum) : IPolicyAckLedgerStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyAckLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NamePolicyAckLedgerAdjuster(Dictionary<string, double> Factors) : IPolicyAckLedgerStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PolicyAckLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the policy ack ledger steps in order and keeps a trace of each.</summary>
public sealed class PolicyAckLedgerEngine(List<IPolicyAckLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyAckLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyAckLedgerEngine DefaultPolicyAckLedgerEngine(Registry flags)
    {
        return new PolicyAckLedgerEngine(
            new List<IPolicyAckLedgerStep>
            {
                new FlaggedPolicyAckLedgerAdjuster(flags, "policy-ack-ledger-legacy", 4.2),
                new PercentPolicyAckLedgerAdjuster(13.4),
                new RoundingPolicyAckLedgerAdjuster(0.05),
                new MinimumPolicyAckLedgerAdjuster(39.0),
                new NamePolicyAckLedgerAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.82, ["central"] = 1.33, ["retail"] = 1.29 }),
            },
            new List<string>(),
            3.5,
            94051.5);
    }

    public double Compute(PolicyAckLedger item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyAckLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
