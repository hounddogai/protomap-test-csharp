namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal career path digest calculation.</summary>
public interface ISeasonalCareerPathDigestFactor
{
    string Describe();
    double Adjust(SeasonalCareerPathDigest item, double valueValue);
}

public sealed record TieredSeasonalCareerPathDigestAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalCareerPathDigestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalCareerPathDigest item, double valueValue)
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

public sealed record SeasonalCareerPathDigestDiscountOffsetAdjuster(double Weight) : ISeasonalCareerPathDigestFactor
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(SeasonalCareerPathDigest item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapSeasonalCareerPathDigestAdjuster(double Cap) : ISeasonalCareerPathDigestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalCareerPathDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentSeasonalCareerPathDigestAdjuster(double Percent) : ISeasonalCareerPathDigestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalCareerPathDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelSeasonalCareerPathDigestAdjuster(Dictionary<string, double> Factors) : ISeasonalCareerPathDigestFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(SeasonalCareerPathDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal career path digest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalCareerPathDigestEngine(List<ISeasonalCareerPathDigestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalCareerPathDigestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalCareerPathDigestEngine DefaultSeasonalCareerPathDigestEngine()
    {
        return new SeasonalCareerPathDigestEngine(
            new List<ISeasonalCareerPathDigestFactor>
            {
                new TieredSeasonalCareerPathDigestAdjuster(
                    new List<double> { 5098.0, 9018.0 },
                    new List<double> { 0.88, 1.0 }),
                new SeasonalCareerPathDigestDiscountOffsetAdjuster(1.57),
                new CapSeasonalCareerPathDigestAdjuster(31546.0),
                new PercentSeasonalCareerPathDigestAdjuster(-11.7),
                new LabelSeasonalCareerPathDigestAdjuster(new Dictionary<string, double> { ["east"] = 0.71 }),
            },
            new List<string>(),
            2.2,
            99164.0);
    }

    public double Run(SeasonalCareerPathDigest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalCareerPathDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
