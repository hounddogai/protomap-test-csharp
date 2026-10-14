namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient nomination rule calculation.</summary>
public interface ILenientNominationRuleAdjuster
{
    string Describe();
    double Adjust(LenientNominationRule item, double valueValue);
}

public sealed record LenientNominationRuleCostOffsetAdjuster(double Weight) : ILenientNominationRuleAdjuster
{
    public string Describe()
    {
        return $"plus cost times {Weight}";
    }

    public double Adjust(LenientNominationRule item, double valueValue)
    {
        return valueValue + item.Cost * Weight;
    }
}

public sealed record FlaggedLenientNominationRuleAdjuster(Registry Flags, string Flag, double Percent) : ILenientNominationRuleAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientNominationRule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientNominationRuleAdjuster(double Cap) : ILenientNominationRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientNominationRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredLenientNominationRuleAdjuster(List<double> Bounds, List<double> Rates) : ILenientNominationRuleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientNominationRule item, double valueValue)
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

/// <summary>Runs the lenient nomination rule steps in order and keeps a trace of each.</summary>
public sealed class LenientNominationRuleEngine(List<ILenientNominationRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientNominationRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientNominationRuleEngine DefaultLenientNominationRuleEngine(Registry flags)
    {
        return new LenientNominationRuleEngine(
            new List<ILenientNominationRuleAdjuster>
            {
                new LenientNominationRuleCostOffsetAdjuster(1.88),
                new FlaggedLenientNominationRuleAdjuster(flags, "lenient-nomination-rule-scheduled", 7.4),
                new CapLenientNominationRuleAdjuster(5180.0),
                new TieredLenientNominationRuleAdjuster(
                    new List<double> { 1214.0, 4175.0 },
                    new List<double> { 1.17, 0.93 }),
            },
            new List<string>(),
            2.3,
            80134.3);
    }

    public double Compute(LenientNominationRule item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LenientNominationRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
