namespace Acme.Workforce.Workplace.AccessBadge.BadgeRequest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge estimate calculation.</summary>
public interface IAccessBadgeEstimateModifier
{
    string Describe();
    double Adjust(AccessBadgeEstimate item, double valueValue);
}

public sealed record TieredAccessBadgeEstimateAdjuster(List<double> Bounds, List<double> Rates) : IAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AccessBadgeEstimate item, double valueValue)
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

public sealed record CapAccessBadgeEstimateAdjuster(double Cap) : IAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccessBadgeEstimate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentAccessBadgeEstimateAdjuster(double Percent) : IAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeEstimate item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingAccessBadgeEstimateAdjuster(double Step) : IAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeEstimate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TitleAccessBadgeEstimateAdjuster(Dictionary<string, double> Factors) : IAccessBadgeEstimateModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AccessBadgeEstimate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the access badge estimate steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeEstimateEngine(List<IAccessBadgeEstimateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeEstimateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeEstimateEngine DefaultAccessBadgeEstimateEngine()
    {
        return new AccessBadgeEstimateEngine(
            new List<IAccessBadgeEstimateModifier>
            {
                new TieredAccessBadgeEstimateAdjuster(
                    new List<double> { 579.0, 5265.0, 6818.0, 7703.0 },
                    new List<double> { 1.19, 0.85, 1.15, 1.1 }),
                new CapAccessBadgeEstimateAdjuster(21042.0),
                new PercentAccessBadgeEstimateAdjuster(1.6),
                new RoundingAccessBadgeEstimateAdjuster(0.1),
                new TitleAccessBadgeEstimateAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.76, ["east"] = 0.88, ["south"] = 1.18 }),
            },
            new List<string>(),
            1.9,
            24163.8);
    }

    public double Price(AccessBadgeEstimate item)
    {
        var current = item.NetExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeEstimate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
