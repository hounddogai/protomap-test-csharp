namespace Acme.Workforce.Orgdesign.Scenario.Tier;

/// <summary>One step of the seasonal scenario tier calculation.</summary>
public interface ISeasonalScenarioTierStep
{
    string Describe();
    double Adjust(SeasonalScenarioTier item, double valueValue);
}

public sealed record TieredSeasonalScenarioTierAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalScenarioTierStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalScenarioTier item, double valueValue)
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

public sealed record CapSeasonalScenarioTierAdjuster(double Cap) : ISeasonalScenarioTierStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(SeasonalScenarioTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record SeasonalScenarioTierTenureMonthsOffsetAdjuster(double Weight) : ISeasonalScenarioTierStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(SeasonalScenarioTier item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record NameSeasonalScenarioTierAdjuster(Dictionary<string, double> Factors) : ISeasonalScenarioTierStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(SeasonalScenarioTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the seasonal scenario tier steps in order and keeps a trace of each.</summary>
public sealed class SeasonalScenarioTierEngine(List<ISeasonalScenarioTierStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalScenarioTierStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalScenarioTierEngine DefaultSeasonalScenarioTierEngine()
    {
        return new SeasonalScenarioTierEngine(
            new List<ISeasonalScenarioTierStep>
            {
                new TieredSeasonalScenarioTierAdjuster(
                    new List<double> { 4608.0, 5698.0, 7781.0, 8862.0 },
                    new List<double> { 1.14, 1.02, 0.84, 0.91 }),
                new CapSeasonalScenarioTierAdjuster(43446.0),
                new SeasonalScenarioTierTenureMonthsOffsetAdjuster(0.56),
                new NameSeasonalScenarioTierAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.83, ["central"] = 0.77 }),
            },
            new List<string>(),
            4.0,
            93172.7);
    }

    public double Evaluate(SeasonalScenarioTier item)
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalScenarioTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
