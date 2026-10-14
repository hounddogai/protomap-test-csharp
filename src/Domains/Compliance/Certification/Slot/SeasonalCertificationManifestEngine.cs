namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal certification manifest calculation.</summary>
public interface ISeasonalCertificationManifestModifier
{
    string Describe();
    double Adjust(SeasonalCertificationManifest item, double valueValue);
}

public sealed record RoundingSeasonalCertificationManifestAdjuster(double Step) : ISeasonalCertificationManifestModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalCertificationManifest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TeamSeasonalCertificationManifestAdjuster(Dictionary<string, double> Factors) : ISeasonalCertificationManifestModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(SeasonalCertificationManifest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentSeasonalCertificationManifestAdjuster(double Percent) : ISeasonalCertificationManifestModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalCertificationManifest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalCertificationManifestAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalCertificationManifestModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalCertificationManifest item, double valueValue)
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

public sealed record MinimumSeasonalCertificationManifestAdjuster(double Minimum) : ISeasonalCertificationManifestModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalCertificationManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal certification manifest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalCertificationManifestEngine(List<ISeasonalCertificationManifestModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalCertificationManifestModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalCertificationManifestEngine DefaultSeasonalCertificationManifestEngine()
    {
        return new SeasonalCertificationManifestEngine(
            new List<ISeasonalCertificationManifestModifier>
            {
                new RoundingSeasonalCertificationManifestAdjuster(5.0),
                new TeamSeasonalCertificationManifestAdjuster(new Dictionary<string, double> { ["west"] = 1.12 }),
                new PercentSeasonalCertificationManifestAdjuster(19.8),
                new TieredSeasonalCertificationManifestAdjuster(
                    new List<double> { 3345.0, 3424.0, 9716.0 },
                    new List<double> { 0.99, 1.16, 0.9 }),
                new MinimumSeasonalCertificationManifestAdjuster(48.7),
            },
            new List<string>(),
            2.0,
            20954.1);
    }

    public double Price(SeasonalCertificationManifest item)
    {
        var current = item.ProjectedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalCertificationManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
