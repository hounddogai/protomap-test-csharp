namespace Acme.Workforce.Workplace.AccessBadge.Metric;

/// <summary>One step of the seasonal access badge digest calculation.</summary>
public interface ISeasonalAccessBadgeDigestAdjuster
{
    string Describe();
    double Adjust(SeasonalAccessBadgeDigest item, double valueValue);
}

public sealed record SeasonalAccessBadgeDigestPriorityOffsetAdjuster(double Weight) : ISeasonalAccessBadgeDigestAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(SeasonalAccessBadgeDigest item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

public sealed record MinimumSeasonalAccessBadgeDigestAdjuster(double Minimum) : ISeasonalAccessBadgeDigestAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalAccessBadgeDigest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapSeasonalAccessBadgeDigestAdjuster(double Cap) : ISeasonalAccessBadgeDigestAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalAccessBadgeDigest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the seasonal access badge digest steps in order and keeps a trace of each.</summary>
public sealed class SeasonalAccessBadgeDigestEngine(List<ISeasonalAccessBadgeDigestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalAccessBadgeDigestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalAccessBadgeDigestEngine DefaultSeasonalAccessBadgeDigestEngine()
    {
        return new SeasonalAccessBadgeDigestEngine(
            new List<ISeasonalAccessBadgeDigestAdjuster>
            {
                new SeasonalAccessBadgeDigestPriorityOffsetAdjuster(0.4),
                new MinimumSeasonalAccessBadgeDigestAdjuster(38.9),
                new CapSeasonalAccessBadgeDigestAdjuster(12253.0),
            },
            new List<string>(),
            4.2,
            43996.0);
    }

    public double Compute(SeasonalAccessBadgeDigest item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalAccessBadgeDigest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
