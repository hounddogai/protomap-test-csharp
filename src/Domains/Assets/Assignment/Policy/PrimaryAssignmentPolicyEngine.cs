namespace Acme.Workforce.Assets.Assignment.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary assignment policy calculation.</summary>
public interface IPrimaryAssignmentPolicyFactor
{
    string Describe();
    double Adjust(PrimaryAssignmentPolicy item, double valueValue);
}

public sealed record TieredPrimaryAssignmentPolicyAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryAssignmentPolicy item, double valueValue)
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

public sealed record FlaggedPrimaryAssignmentPolicyAdjuster(Registry Flags, string Flag, double Percent) : IPrimaryAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PrimaryAssignmentPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentPrimaryAssignmentPolicyAdjuster(double Percent) : IPrimaryAssignmentPolicyFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryAssignmentPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NamePrimaryAssignmentPolicyAdjuster(Dictionary<string, double> Factors) : IPrimaryAssignmentPolicyFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PrimaryAssignmentPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the primary assignment policy steps in order and keeps a trace of each.</summary>
public sealed class PrimaryAssignmentPolicyEngine(List<IPrimaryAssignmentPolicyFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryAssignmentPolicyFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryAssignmentPolicyEngine DefaultPrimaryAssignmentPolicyEngine(Registry flags)
    {
        return new PrimaryAssignmentPolicyEngine(
            new List<IPrimaryAssignmentPolicyFactor>
            {
                new TieredPrimaryAssignmentPolicyAdjuster(
                    new List<double> { 932.0, 5176.0 },
                    new List<double> { 0.93, 1.0 }),
                new FlaggedPrimaryAssignmentPolicyAdjuster(flags, "primary-assignment-policy-manual", -5.6),
                new PercentPrimaryAssignmentPolicyAdjuster(-0.1),
                new NamePrimaryAssignmentPolicyAdjuster(
                    new Dictionary<string, double> { ["retail"] = 1.33, ["west"] = 1.28 }),
            },
            new List<string>(),
            1.8,
            10167.5);
    }

    public double Evaluate(PrimaryAssignmentPolicy item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryAssignmentPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
