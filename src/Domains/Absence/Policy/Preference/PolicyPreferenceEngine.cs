namespace Acme.Workforce.Absence.Policy.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy preference calculation.</summary>
public interface IPolicyPreferenceStep
{
    string Describe();
    double Adjust(PolicyPreference item, double valueValue);
}

public sealed record TitlePolicyPreferenceAdjuster(Dictionary<string, double> Factors) : IPolicyPreferenceStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PolicyPreference item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPolicyPreferenceAdjuster(Registry Flags, string Flag, double Percent) : IPolicyPreferenceStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PolicyPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPolicyPreferenceAdjuster(double Cap) : IPolicyPreferenceStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyPreference item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredPolicyPreferenceAdjuster(List<double> Bounds, List<double> Rates) : IPolicyPreferenceStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyPreference item, double valueValue)
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

public sealed record PolicyPreferenceLeaveDaysOffsetAdjuster(double Weight) : IPolicyPreferenceStep
{
    public string Describe()
    {
        return $"plus leave days times {Weight}";
    }

    public double Adjust(PolicyPreference item, double valueValue)
    {
        return valueValue + (double)item.LeaveDays * Weight;
    }
}

/// <summary>Runs the policy preference steps in order and keeps a trace of each.</summary>
public sealed class PolicyPreferenceEngine(List<IPolicyPreferenceStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyPreferenceStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyPreferenceEngine DefaultPolicyPreferenceEngine(Registry flags)
    {
        return new PolicyPreferenceEngine(
            new List<IPolicyPreferenceStep>
            {
                new TitlePolicyPreferenceAdjuster(new Dictionary<string, double> { ["north"] = 1.1 }),
                new FlaggedPolicyPreferenceAdjuster(flags, "policy-preference-legacy", 9.4),
                new CapPolicyPreferenceAdjuster(49404.0),
                new TieredPolicyPreferenceAdjuster(
                    new List<double> { 2626.0, 4840.0, 9834.0 },
                    new List<double> { 1.07, 0.94, 1.08 }),
                new PolicyPreferenceLeaveDaysOffsetAdjuster(1.77),
            },
            new List<string>(),
            0.8,
            61952.1);
    }

    public double Run(PolicyPreference item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
