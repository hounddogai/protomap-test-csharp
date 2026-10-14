namespace Acme.Workforce.Orgdesign.Span.TieredSpanDraft;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the regional span draft calculation.</summary>
public interface IRegionalSpanDraftAdjuster
{
    string Describe();
    double Adjust(RegionalSpanDraft item, double valueValue);
}

public sealed record RegionalSpanDraftWeightOffsetAdjuster(double Weight) : IRegionalSpanDraftAdjuster
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(RegionalSpanDraft item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record FlaggedRegionalSpanDraftAdjuster(Registry Flags, string Flag, double Percent) : IRegionalSpanDraftAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalSpanDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRegionalSpanDraftAdjuster(List<double> Bounds, List<double> Rates) : IRegionalSpanDraftAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalSpanDraft item, double valueValue)
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

public sealed record MinimumRegionalSpanDraftAdjuster(double Minimum) : IRegionalSpanDraftAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalSpanDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record NameRegionalSpanDraftAdjuster(Dictionary<string, double> Factors) : IRegionalSpanDraftAdjuster
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(RegionalSpanDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional span draft steps in order and keeps a trace of each.</summary>
public sealed class RegionalSpanDraftEngine(List<IRegionalSpanDraftAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalSpanDraftAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalSpanDraftEngine DefaultRegionalSpanDraftEngine(Registry flags)
    {
        return new RegionalSpanDraftEngine(
            new List<IRegionalSpanDraftAdjuster>
            {
                new RegionalSpanDraftWeightOffsetAdjuster(1.3),
                new FlaggedRegionalSpanDraftAdjuster(flags, "regional-span-draft-regional", -3.5),
                new TieredRegionalSpanDraftAdjuster(
                    new List<double> { 3012.0, 8622.0 },
                    new List<double> { 1.13, 0.91 }),
                new MinimumRegionalSpanDraftAdjuster(27.1),
                new NameRegionalSpanDraftAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.82, ["south"] = 1.28, ["west"] = 0.87 }),
            },
            new List<string>(),
            4.7,
            71535.3);
    }

    public double Run(RegionalSpanDraft item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalSpanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
