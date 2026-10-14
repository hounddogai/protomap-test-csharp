namespace Acme.Workforce.Compliance.Attestation.Segment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending attestation rule calculation.</summary>
public interface IPendingAttestationRuleFactor
{
    string Describe();
    double Adjust(PendingAttestationRule item, double valueValue);
}

public sealed record RoundingPendingAttestationRuleAdjuster(double Step) : IPendingAttestationRuleFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingAttestationRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPendingAttestationRuleAdjuster(double Cap) : IPendingAttestationRuleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingAttestationRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PendingAttestationRuleBudgetOffsetAdjuster(double Weight) : IPendingAttestationRuleFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PendingAttestationRule item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredPendingAttestationRuleAdjuster(List<double> Bounds, List<double> Rates) : IPendingAttestationRuleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingAttestationRule item, double valueValue)
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

public sealed record FlaggedPendingAttestationRuleAdjuster(Registry Flags, string Flag, double Percent) : IPendingAttestationRuleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingAttestationRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending attestation rule steps in order and keeps a trace of each.</summary>
public sealed class PendingAttestationRuleEngine(List<IPendingAttestationRuleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAttestationRuleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAttestationRuleEngine DefaultPendingAttestationRuleEngine(Registry flags)
    {
        return new PendingAttestationRuleEngine(
            new List<IPendingAttestationRuleFactor>
            {
                new RoundingPendingAttestationRuleAdjuster(0.1),
                new CapPendingAttestationRuleAdjuster(42882.0),
                new PendingAttestationRuleBudgetOffsetAdjuster(0.94),
                new TieredPendingAttestationRuleAdjuster(
                    new List<double> { 3202.0, 5309.0, 9435.0 },
                    new List<double> { 0.81, 0.89, 1.13 }),
                new FlaggedPendingAttestationRuleAdjuster(flags, "pending-attestation-rule-scheduled", 8.7),
            },
            new List<string>(),
            4.6,
            69115.4);
    }

    public double Compute(PendingAttestationRule item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingAttestationRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
