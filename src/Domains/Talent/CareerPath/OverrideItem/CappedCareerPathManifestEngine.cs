namespace Acme.Workforce.Talent.CareerPath.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped career path manifest calculation.</summary>
public interface ICappedCareerPathManifestModifier
{
    string Describe();
    double Adjust(CappedCareerPathManifest item, double valueValue);
}

public sealed record RoundingCappedCareerPathManifestAdjuster(double Step) : ICappedCareerPathManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedCareerPathManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CappedCareerPathManifestHoursOffsetAdjuster(double Weight) : ICappedCareerPathManifestModifier
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(CappedCareerPathManifest item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

public sealed record RegionCappedCareerPathManifestAdjuster(Dictionary<string, double> Factors) : ICappedCareerPathManifestModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(CappedCareerPathManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentCappedCareerPathManifestAdjuster(double Percent) : ICappedCareerPathManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedCareerPathManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the capped career path manifest steps in order and keeps a trace of each.</summary>
public sealed class CappedCareerPathManifestEngine(List<ICappedCareerPathManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedCareerPathManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedCareerPathManifestEngine DefaultCappedCareerPathManifestEngine()
    {
        return new CappedCareerPathManifestEngine(
            new List<ICappedCareerPathManifestModifier>
            {
                new RoundingCappedCareerPathManifestAdjuster(5.0),
                new CappedCareerPathManifestHoursOffsetAdjuster(1.96),
                new RegionCappedCareerPathManifestAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.86, ["west"] = 1.25 }),
                new PercentCappedCareerPathManifestAdjuster(-7.8),
            },
            new List<string>(),
            0.7,
            33915.9);
    }

    public double Price(CappedCareerPathManifest item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<CappedCareerPathManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
