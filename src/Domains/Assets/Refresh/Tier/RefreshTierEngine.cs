namespace Acme.Workforce.Assets.Refresh.Tier;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh tier calculation.</summary>
public interface IRefreshTierFactor
{
    string Describe();
    double Adjust(RefreshTier item, double valueValue);
}

public sealed record RoundingRefreshTierAdjuster(double Step) : IRefreshTierFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRefreshTierAdjuster(List<double> Bounds, List<double> Rates) : IRefreshTierFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshTier item, double valueValue)
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

public sealed record MinimumRefreshTierAdjuster(double Minimum) : IRefreshTierFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshTier item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TitleRefreshTierAdjuster(Dictionary<string, double> Factors) : IRefreshTierFactor
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(RefreshTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RefreshTierHoursOffsetAdjuster(double Weight) : IRefreshTierFactor
{
    public string Describe()
    {
        return $"plus hours times {Weight}";
    }

    public double Adjust(RefreshTier item, double valueValue)
    {
        return valueValue + item.Hours * Weight;
    }
}

/// <summary>Runs the refresh tier steps in order and keeps a trace of each.</summary>
public sealed class RefreshTierEngine(List<IRefreshTierFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshTierFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshTierEngine DefaultRefreshTierEngine()
    {
        return new RefreshTierEngine(
            new List<IRefreshTierFactor>
            {
                new RoundingRefreshTierAdjuster(0.25),
                new TieredRefreshTierAdjuster(new List<double> { 7426.0, 9028.0 }, new List<double> { 0.86, 1.19 }),
                new MinimumRefreshTierAdjuster(36.1),
                new TitleRefreshTierAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.12, ["east"] = 0.83, ["online"] = 1.16 }),
                new RefreshTierHoursOffsetAdjuster(0.82),
            },
            new List<string>(),
            4.7,
            49762.7);
    }

    public double Compute(RefreshTier item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
