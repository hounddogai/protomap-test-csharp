namespace Acme.Workforce.Workplace.AccessBadge.Manifest;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped access badge manifest calculation.</summary>
public interface ICappedAccessBadgeManifestModifier
{
    string Describe();
    double Adjust(CappedAccessBadgeManifest item, double valueValue);
}

public sealed record CapCappedAccessBadgeManifestAdjuster(double Cap) : ICappedAccessBadgeManifestModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAccessBadgeManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentCappedAccessBadgeManifestAdjuster(double Percent) : ICappedAccessBadgeManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedAccessBadgeManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryCappedAccessBadgeManifestAdjuster(Dictionary<string, double> Factors) : ICappedAccessBadgeManifestModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(CappedAccessBadgeManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumCappedAccessBadgeManifestAdjuster(double Minimum) : ICappedAccessBadgeManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(CappedAccessBadgeManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingCappedAccessBadgeManifestAdjuster(double Step) : ICappedAccessBadgeManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAccessBadgeManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped access badge manifest steps in order and keeps a trace of each.</summary>
public sealed class CappedAccessBadgeManifestEngine(List<ICappedAccessBadgeManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAccessBadgeManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAccessBadgeManifestEngine DefaultCappedAccessBadgeManifestEngine()
    {
        return new CappedAccessBadgeManifestEngine(
            new List<ICappedAccessBadgeManifestModifier>
            {
                new CapCappedAccessBadgeManifestAdjuster(16591.0),
                new PercentCappedAccessBadgeManifestAdjuster(15.5),
                new CategoryCappedAccessBadgeManifestAdjuster(new Dictionary<string, double> { ["online"] = 0.78 }),
                new MinimumCappedAccessBadgeManifestAdjuster(6.9),
                new RoundingCappedAccessBadgeManifestAdjuster(0.05),
            },
            new List<string>(),
            3.6,
            6057.4);
    }

    public double Price(CappedAccessBadgeManifest item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAccessBadgeManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
