namespace Acme.Workforce.Absence.Policy.Score;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the policy manifest calculation.</summary>
public interface IPolicyManifestModifier
{
    string Describe();
    double Adjust(PolicyManifest item, double valueValue);
}

public sealed record CapPolicyManifestAdjuster(double Cap) : IPolicyManifestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PolicyManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPolicyManifestAdjuster(double Percent) : IPolicyManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PolicyManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredPolicyManifestAdjuster(List<double> Bounds, List<double> Rates) : IPolicyManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PolicyManifest item, double valueValue)
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

public sealed record MinimumPolicyManifestAdjuster(double Minimum) : IPolicyManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PolicyManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryPolicyManifestAdjuster(Dictionary<string, double> Factors) : IPolicyManifestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(PolicyManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the policy manifest steps in order and keeps a trace of each.</summary>
public sealed class PolicyManifestEngine(List<IPolicyManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPolicyManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PolicyManifestEngine DefaultPolicyManifestEngine()
    {
        return new PolicyManifestEngine(
            new List<IPolicyManifestModifier>
            {
                new CapPolicyManifestAdjuster(15717.0),
                new PercentPolicyManifestAdjuster(-1.9),
                new TieredPolicyManifestAdjuster(
                    new List<double> { 1244.0, 2917.0, 7144.0, 7223.0 },
                    new List<double> { 0.87, 0.89, 0.96, 1.15 }),
                new MinimumPolicyManifestAdjuster(22.1),
                new CategoryPolicyManifestAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.19, ["retail"] = 1.0 }),
            },
            new List<string>(),
            0.2,
            67160.0);
    }

    public double Compute(PolicyManifest item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<PolicyManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
