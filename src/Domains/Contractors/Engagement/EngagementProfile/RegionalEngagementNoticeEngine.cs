namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional engagement notice calculation.</summary>
public interface IRegionalEngagementNoticeModifier
{
    string Describe();
    double Adjust(RegionalEngagementNotice item, double valueValue);
}

public sealed record RegionalEngagementNoticeDiscountOffsetAdjuster(double Weight) : IRegionalEngagementNoticeModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(RegionalEngagementNotice item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record TitleRegionalEngagementNoticeAdjuster(Dictionary<string, double> Factors) : IRegionalEngagementNoticeModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RegionalEngagementNotice item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentRegionalEngagementNoticeAdjuster(double Percent) : IRegionalEngagementNoticeModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalEngagementNotice item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapRegionalEngagementNoticeAdjuster(double Cap) : IRegionalEngagementNoticeModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalEngagementNotice item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredRegionalEngagementNoticeAdjuster(List<double> Bounds, List<double> Rates) : IRegionalEngagementNoticeModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalEngagementNotice item, double valueValue)
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

public sealed record RoundingRegionalEngagementNoticeAdjuster(double Step) : IRegionalEngagementNoticeModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalEngagementNotice item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the regional engagement notice steps in order and keeps a trace of each.</summary>
public sealed class RegionalEngagementNoticeEngine(List<IRegionalEngagementNoticeModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalEngagementNoticeModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalEngagementNoticeEngine DefaultRegionalEngagementNoticeEngine()
    {
        return new RegionalEngagementNoticeEngine(
            new List<IRegionalEngagementNoticeModifier>
            {
                new RegionalEngagementNoticeDiscountOffsetAdjuster(0.73),
                new TitleRegionalEngagementNoticeAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.21, ["north"] = 1.0, ["east"] = 0.94 }),
                new PercentRegionalEngagementNoticeAdjuster(0.2),
                new CapRegionalEngagementNoticeAdjuster(29362.0),
                new TieredRegionalEngagementNoticeAdjuster(
                    new List<double> { 7117.0, 8726.0 },
                    new List<double> { 0.83, 1.0 }),
                new RoundingRegionalEngagementNoticeAdjuster(5.0),
            },
            new List<string>(),
            0.4,
            19567.8);
    }

    public double Evaluate(RegionalEngagementNotice item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalEngagementNotice> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
