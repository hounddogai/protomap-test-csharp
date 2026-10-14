namespace Acme.Workforce.Workplace.AccessBadge.BadgeLedger;

/// <summary>One step of the seasonal access badge draft calculation.</summary>
public interface ISeasonalAccessBadgeDraftStep
{
    string Describe();
    double Adjust(SeasonalAccessBadgeDraft item, double valueValue);
}

public sealed record TieredSeasonalAccessBadgeDraftAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalAccessBadgeDraft item, double valueValue)
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

public sealed record SeasonalAccessBadgeDraftStartsAtOffsetAdjuster(double Weight) : ISeasonalAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(SeasonalAccessBadgeDraft item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

public sealed record TitleSeasonalAccessBadgeDraftAdjuster(Dictionary<string, double> Factors) : ISeasonalAccessBadgeDraftStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(SeasonalAccessBadgeDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapSeasonalAccessBadgeDraftAdjuster(double Cap) : ISeasonalAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAccessBadgeDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumSeasonalAccessBadgeDraftAdjuster(double Minimum) : ISeasonalAccessBadgeDraftStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalAccessBadgeDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal access badge draft steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAccessBadgeDraftEngine(List<ISeasonalAccessBadgeDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAccessBadgeDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAccessBadgeDraftEngine DefaultSeasonalAccessBadgeDraftEngine()
    {
        return new SeasonalAccessBadgeDraftEngine(
            new List<ISeasonalAccessBadgeDraftStep>
            {
                new TieredSeasonalAccessBadgeDraftAdjuster(
                    new List<double> { 451.0, 4947.0, 6518.0 },
                    new List<double> { 1.17, 1.08, 1.16 }),
                new SeasonalAccessBadgeDraftStartsAtOffsetAdjuster(1.34),
                new TitleSeasonalAccessBadgeDraftAdjuster(new Dictionary<string, double> { ["west"] = 1.07 }),
                new CapSeasonalAccessBadgeDraftAdjuster(19550.0),
                new MinimumSeasonalAccessBadgeDraftAdjuster(38.8),
            },
            new List<string>(),
            2.5,
            44239.3);
    }

    public double Run(SeasonalAccessBadgeDraft item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAccessBadgeDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
