namespace Acme.Workforce.Compensation.PayBand.Estimate;

/// <summary>One step of the seasonal pay band quota calculation.</summary>
public interface ISeasonalPayBandQuotaModifier
{
    string Describe();
    double Adjust(SeasonalPayBandQuota item, double valueValue);
}

public sealed record PercentSeasonalPayBandQuotaAdjuster(double Percent) : ISeasonalPayBandQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalPayBandQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RegionSeasonalPayBandQuotaAdjuster(Dictionary<string, double> Factors) : ISeasonalPayBandQuotaModifier
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(SeasonalPayBandQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumSeasonalPayBandQuotaAdjuster(double Minimum) : ISeasonalPayBandQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalPayBandQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the seasonal pay band quota steps in order and keeps a trace of each.</summary>
public sealed class SeasonalPayBandQuotaEngine(List<ISeasonalPayBandQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalPayBandQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalPayBandQuotaEngine DefaultSeasonalPayBandQuotaEngine()
    {
        return new SeasonalPayBandQuotaEngine(
            new List<ISeasonalPayBandQuotaModifier>
            {
                new PercentSeasonalPayBandQuotaAdjuster(-17.8),
                new RegionSeasonalPayBandQuotaAdjuster(new Dictionary<string, double> { ["south"] = 1.24 }),
                new MinimumSeasonalPayBandQuotaAdjuster(12.2),
            },
            new List<string>(),
            0.1,
            60126.3);
    }

    public double Run(SeasonalPayBandQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalPayBandQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
