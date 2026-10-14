namespace Acme.Workforce.Talent.Nomination.NominationPolicy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional nomination policy calculation.</summary>
public interface IRegionalNominationPolicyAdjuster
{
    string Describe();
    double Adjust(RegionalNominationPolicy item, double valueValue);
}

public sealed record PercentRegionalNominationPolicyAdjuster(double Percent) : IRegionalNominationPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalNominationPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalNominationPolicyAdjuster(double Step) : IRegionalNominationPolicyAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalNominationPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalNominationPolicyAdjuster(List<double> Bounds, List<double> Rates) : IRegionalNominationPolicyAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalNominationPolicy item, double valueValue)
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

public sealed record CategoryRegionalNominationPolicyAdjuster(Dictionary<string, double> Factors) : IRegionalNominationPolicyAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RegionalNominationPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRegionalNominationPolicyAdjuster(Registry Flags, string Flag, double Percent) : IRegionalNominationPolicyAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalNominationPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional nomination policy steps in order and keeps a trace of each.</summary>
public sealed class RegionalNominationPolicyEngine(List<IRegionalNominationPolicyAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalNominationPolicyAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalNominationPolicyEngine DefaultRegionalNominationPolicyEngine(Registry flags)
    {
        return new RegionalNominationPolicyEngine(
            new List<IRegionalNominationPolicyAdjuster>
            {
                new PercentRegionalNominationPolicyAdjuster(11.8),
                new RoundingRegionalNominationPolicyAdjuster(0.25),
                new TieredRegionalNominationPolicyAdjuster(
                    new List<double> { 1171.0, 1713.0, 6113.0 },
                    new List<double> { 1.1, 1.08, 1.03 }),
                new CategoryRegionalNominationPolicyAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.29, ["south"] = 0.74 }),
                new FlaggedRegionalNominationPolicyAdjuster(flags, "regional-nomination-policy-tiered", 1.8),
            },
            new List<string>(),
            2.2,
            55096.5);
    }

    public double Run(RegionalNominationPolicy item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalNominationPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
