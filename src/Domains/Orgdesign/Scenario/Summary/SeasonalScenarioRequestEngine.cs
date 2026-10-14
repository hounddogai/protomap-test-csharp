namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal scenario request calculation.</summary>
public interface ISeasonalScenarioRequestAdjuster
{
    string Describe();
    double Adjust(SeasonalScenarioRequest item, double valueValue);
}

public sealed record RoundingSeasonalScenarioRequestAdjuster(double Step) : ISeasonalScenarioRequestAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SeasonalScenarioRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSeasonalScenarioRequestAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalScenarioRequestAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalScenarioRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredSeasonalScenarioRequestAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalScenarioRequestAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalScenarioRequest item, double valueValue)
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

public sealed record SeasonalScenarioRequestPriorityOffsetAdjuster(double Weight) : ISeasonalScenarioRequestAdjuster
{
    public string Describe()
    {
        return $"plus priority times {Weight}";
    }

    public double Adjust(SeasonalScenarioRequest item, double valueValue)
    {
        return valueValue + (double)item.Priority * Weight;
    }
}

/// <summary>Runs the seasonal scenario request steps in order and keeps a trace of each.</summary>
public sealed class SeasonalScenarioRequestEngine(List<ISeasonalScenarioRequestAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalScenarioRequestAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalScenarioRequestEngine DefaultSeasonalScenarioRequestEngine(Registry flags)
    {
        return new SeasonalScenarioRequestEngine(
            new List<ISeasonalScenarioRequestAdjuster>
            {
                new RoundingSeasonalScenarioRequestAdjuster(0.05),
                new FlaggedSeasonalScenarioRequestAdjuster(flags, "seasonal-scenario-request-rolling", -2.3),
                new TieredSeasonalScenarioRequestAdjuster(
                    new List<double> { 232.0, 244.0, 2434.0, 4222.0 },
                    new List<double> { 1.06, 1.09, 0.84, 1.0 }),
                new SeasonalScenarioRequestPriorityOffsetAdjuster(0.78),
            },
            new List<string>(),
            0.4,
            14679.3);
    }

    public double Evaluate(SeasonalScenarioRequest item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalScenarioRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
