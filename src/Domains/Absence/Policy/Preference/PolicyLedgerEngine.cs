namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy ledger calculation.</summary>
public interface IPolicyLedgerFactor
{
    string Describe();
    double Adjust(PolicyLedger item, double valueValue);
}

public sealed record CapPolicyLedgerAdjuster(double Cap) : IPolicyLedgerFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumPolicyLedgerAdjuster(double Minimum) : IPolicyLedgerFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyLedger item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPolicyLedgerAdjuster(double Step) : IPolicyLedgerFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the policy ledger steps in order and keeps a trace of each.</summary>
public sealed class PolicyLedgerEngine(List<IPolicyLedgerFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyLedgerFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyLedgerEngine DefaultPolicyLedgerEngine()
    {
        return new PolicyLedgerEngine(
            new List<IPolicyLedgerFactor>
            {
                new CapPolicyLedgerAdjuster(43066.0),
                new MinimumPolicyLedgerAdjuster(33.1),
                new RoundingPolicyLedgerAdjuster(0.25),
            },
            new List<string>(),
            0.8,
            95970.7);
    }

    public double Compute(PolicyLedger item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
