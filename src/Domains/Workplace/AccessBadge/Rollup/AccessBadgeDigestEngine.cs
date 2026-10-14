namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge digest calculation.</summary>
public interface IAccessBadgeDigestModifier
{
    string Describe();
    double Adjust(AccessBadgeDigest item, double valueValue);
}

public sealed record MinimumAccessBadgeDigestAdjuster(double Minimum) : IAccessBadgeDigestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredAccessBadgeDigestAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeDigestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeDigest item, double valueValue)
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

public sealed record PercentAccessBadgeDigestAdjuster(double Percent) : IAccessBadgeDigestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAccessBadgeDigestAdjuster(Dictionary<string, double> Factors) : IAccessBadgeDigestModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AccessBadgeDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAccessBadgeDigestAdjuster(double Cap) : IAccessBadgeDigestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the access badge digest steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeDigestEngine(List<IAccessBadgeDigestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeDigestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeDigestEngine DefaultAccessBadgeDigestEngine()
    {
        return new AccessBadgeDigestEngine(
            new List<IAccessBadgeDigestModifier>
            {
                new MinimumAccessBadgeDigestAdjuster(38.3),
                new TieredAccessBadgeDigestAdjuster(
                    new List<double> { 3154.0, 9991.0 },
                    new List<double> { 0.89, 1.19 }),
                new PercentAccessBadgeDigestAdjuster(-11.3),
                new NameAccessBadgeDigestAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.72, ["south"] = 0.73 }),
                new CapAccessBadgeDigestAdjuster(31624.0),
            },
            new List<string>(),
            2.9,
            14958.8);
    }

    public double Run(AccessBadgeDigest item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
