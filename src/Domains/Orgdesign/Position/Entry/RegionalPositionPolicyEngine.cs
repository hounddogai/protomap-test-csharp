namespace Acme.Workforce.Orgdesign.Position.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional position policy calculation.</summary>
public interface IRegionalPositionPolicyModifier
{
    string Describe();
    double Adjust(RegionalPositionPolicy item, double valueValue);
}

public sealed record PercentRegionalPositionPolicyAdjuster(double Percent) : IRegionalPositionPolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalPositionPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalPositionPolicyAdjuster(Registry Flags, string Flag, double Percent) : IRegionalPositionPolicyModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalPositionPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalPositionPolicyAdjuster(double Cap) : IRegionalPositionPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalPositionPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRegionalPositionPolicyAdjuster(List<double> Bounds, List<double> Rates) : IRegionalPositionPolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalPositionPolicy item, double valueValue)
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

public sealed record RoundingRegionalPositionPolicyAdjuster(double Step) : IRegionalPositionPolicyModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalPositionPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelRegionalPositionPolicyAdjuster(Dictionary<string, double> Factors) : IRegionalPositionPolicyModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(RegionalPositionPolicy item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional position policy steps in order and keeps a trace of each.</summary>
public sealed class RegionalPositionPolicyEngine(List<IRegionalPositionPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalPositionPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalPositionPolicyEngine DefaultRegionalPositionPolicyEngine(Registry flags)
    {
        return new RegionalPositionPolicyEngine(
            new List<IRegionalPositionPolicyModifier>
            {
                new PercentRegionalPositionPolicyAdjuster(-8.3),
                new FlaggedRegionalPositionPolicyAdjuster(flags, "regional-position-policy-fallback", 1.3),
                new CapRegionalPositionPolicyAdjuster(31574.0),
                new TieredRegionalPositionPolicyAdjuster(
                    new List<double> { 120.0, 1090.0, 3719.0 },
                    new List<double> { 0.94, 1.04, 1.1 }),
                new RoundingRegionalPositionPolicyAdjuster(0.1),
                new LabelRegionalPositionPolicyAdjuster(
                    new Dictionary<string, double> { ["north"] = 1.25, ["west"] = 1.27 }),
            },
            new List<string>(),
            4.7,
            92605.4);
    }

    public double Run(RegionalPositionPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalPositionPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
