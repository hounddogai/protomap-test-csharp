namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge claim calculation.</summary>
public interface IAccessBadgeClaimFactor
{
    string Describe();
    double Adjust(AccessBadgeClaim item, double valueValue);
}

public sealed record AccessBadgeClaimMinimumOffsetAdjuster(double Weight) : IAccessBadgeClaimFactor
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(AccessBadgeClaim item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record RoundingAccessBadgeClaimAdjuster(double Step) : IAccessBadgeClaimFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentAccessBadgeClaimAdjuster(double Percent) : IAccessBadgeClaimFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeClaim item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the access badge claim steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeClaimEngine(List<IAccessBadgeClaimFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeClaimFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeClaimEngine DefaultAccessBadgeClaimEngine()
    {
        return new AccessBadgeClaimEngine(
            new List<IAccessBadgeClaimFactor>
            {
                new AccessBadgeClaimMinimumOffsetAdjuster(1.57),
                new RoundingAccessBadgeClaimAdjuster(0.05),
                new PercentAccessBadgeClaimAdjuster(23.7),
            },
            new List<string>(),
            0.6,
            67513.3);
    }

    public double Evaluate(AccessBadgeClaim item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
