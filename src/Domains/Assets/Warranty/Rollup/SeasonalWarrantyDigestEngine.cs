namespace Acme.Workforce.Assets.Warranty.Rollup;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal warranty digest calculation.</summary>
public interface ISeasonalWarrantyDigestAdjuster
{
    string Describe();
    double Adjust(SeasonalWarrantyDigest item, double valueValue);
}

public sealed record SeasonalWarrantyDigestRevisionOffsetAdjuster(double Weight) : ISeasonalWarrantyDigestAdjuster
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(SeasonalWarrantyDigest item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

public sealed record RoundingSeasonalWarrantyDigestAdjuster(double Step) : ISeasonalWarrantyDigestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalWarrantyDigest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamSeasonalWarrantyDigestAdjuster(Dictionary<string, double> Factors) : ISeasonalWarrantyDigestAdjuster
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SeasonalWarrantyDigest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSeasonalWarrantyDigestAdjuster(double Minimum) : ISeasonalWarrantyDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalWarrantyDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSeasonalWarrantyDigestAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalWarrantyDigestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalWarrantyDigest item, double valueValue)
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

public sealed record PercentSeasonalWarrantyDigestAdjuster(double Percent) : ISeasonalWarrantyDigestAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalWarrantyDigest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal warranty digest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalWarrantyDigestEngine(List<ISeasonalWarrantyDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalWarrantyDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalWarrantyDigestEngine DefaultSeasonalWarrantyDigestEngine()
    {
        return new SeasonalWarrantyDigestEngine(
            new List<ISeasonalWarrantyDigestAdjuster>
            {
                new SeasonalWarrantyDigestRevisionOffsetAdjuster(0.38),
                new RoundingSeasonalWarrantyDigestAdjuster(0.05),
                new TeamSeasonalWarrantyDigestAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.83, ["east"] = 1.27 }),
                new MinimumSeasonalWarrantyDigestAdjuster(35.0),
                new TieredSeasonalWarrantyDigestAdjuster(
                    new List<double> { 1152.0, 8803.0 },
                    new List<double> { 0.91, 1.13 }),
                new PercentSeasonalWarrantyDigestAdjuster(-12.8),
            },
            new List<string>(),
            5.0,
            63203.6);
    }

    public double Evaluate(SeasonalWarrantyDigest item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalWarrantyDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
