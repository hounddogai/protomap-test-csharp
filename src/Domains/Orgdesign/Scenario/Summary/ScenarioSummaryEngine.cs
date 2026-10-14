namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario summary calculation.</summary>
public interface IScenarioSummaryStep
{
    string Describe();
    double Adjust(ScenarioSummary item, double valueValue);
}

public sealed record FlaggedScenarioSummaryAdjuster(Registry Flags, string Flag, double Percent) : IScenarioSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitleScenarioSummaryAdjuster(Dictionary<string, double> Factors) : IScenarioSummaryStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScenarioSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredScenarioSummaryAdjuster(List<double> Bounds, List<double> Rates) : IScenarioSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioSummary item, double valueValue)
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

public sealed record RoundingScenarioSummaryAdjuster(double Step) : IScenarioSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ScenarioSummaryCapacityOffsetAdjuster(double Weight) : IScenarioSummaryStep
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(ScenarioSummary item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the scenario summary steps in order and keeps a trace of each.</summary>
public sealed class ScenarioSummaryEngine(List<IScenarioSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioSummaryEngine DefaultScenarioSummaryEngine(Registry flags)
    {
        return new ScenarioSummaryEngine(
            new List<IScenarioSummaryStep>
            {
                new FlaggedScenarioSummaryAdjuster(flags, "scenario-summary-manual", 0.3),
                new TitleScenarioSummaryAdjuster(new Dictionary<string, double> { ["south"] = 0.85 }),
                new TieredScenarioSummaryAdjuster(
                    new List<double> { 1193.0, 1537.0, 1553.0, 2458.0 },
                    new List<double> { 0.84, 1.05, 0.87, 1.09 }),
                new RoundingScenarioSummaryAdjuster(0.5),
                new ScenarioSummaryCapacityOffsetAdjuster(1.13),
            },
            new List<string>(),
            1.7,
            57580.5);
    }

    public double Evaluate(ScenarioSummary item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
