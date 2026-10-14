namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy draft calculation.</summary>
public interface IPolicyDraftFactor
{
    string Describe();
    double Adjust(PolicyDraft item, double valueValue);
}

public sealed record PercentPolicyDraftAdjuster(double Percent) : IPolicyDraftFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPolicyDraftAdjuster(Registry Flags, string Flag, double Percent) : IPolicyDraftFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPolicyDraftAdjuster(double Step) : IPolicyDraftFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PolicyDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PolicyDraftBudgetOffsetAdjuster(double Weight) : IPolicyDraftFactor
{
    public string Describe()
    {
        return $"plus budget times {Weight}";
    }

    public double Adjust(PolicyDraft item, double valueValue)
    {
        return valueValue + item.Budget * Weight;
    }
}

public sealed record TieredPolicyDraftAdjuster(List<double> Bounds, List<double> Rates) : IPolicyDraftFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyDraft item, double valueValue)
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

public sealed record ChannelPolicyDraftAdjuster(Dictionary<string, double> Factors) : IPolicyDraftFactor
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(PolicyDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the policy draft steps in order and keeps a trace of each.</summary>
public sealed class PolicyDraftEngine(List<IPolicyDraftFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyDraftFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyDraftEngine DefaultPolicyDraftEngine(Registry flags)
    {
        return new PolicyDraftEngine(
            new List<IPolicyDraftFactor>
            {
                new PercentPolicyDraftAdjuster(10.3),
                new FlaggedPolicyDraftAdjuster(flags, "policy-draft-tiered", -10.0),
                new RoundingPolicyDraftAdjuster(0.25),
                new PolicyDraftBudgetOffsetAdjuster(0.54),
                new TieredPolicyDraftAdjuster(new List<double> { 337.0, 7787.0 }, new List<double> { 1.06, 1.03 }),
                new ChannelPolicyDraftAdjuster(new Dictionary<string, double> { ["south"] = 0.89 }),
            },
            new List<string>(),
            2.5,
            48153.4);
    }

    public double Price(PolicyDraft item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
