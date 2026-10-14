namespace Acme.Workforce.Orgdesign.Scenario.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario batch calculation.</summary>
public interface IScenarioBatchStep
{
    string Describe();
    double Adjust(ScenarioBatch item, double valueValue);
}

public sealed record TieredScenarioBatchAdjuster(List<double> Bounds, List<double> Rates) : IScenarioBatchStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioBatch item, double valueValue)
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

public sealed record RoundingScenarioBatchAdjuster(double Step) : IScenarioBatchStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioBatch item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LabelScenarioBatchAdjuster(Dictionary<string, double> Factors) : IScenarioBatchStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(ScenarioBatch item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentScenarioBatchAdjuster(double Percent) : IScenarioBatchStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioBatch item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scenario batch steps in order and keeps a trace of each.</summary>
public sealed class ScenarioBatchEngine(List<IScenarioBatchStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioBatchStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioBatchEngine DefaultScenarioBatchEngine()
    {
        return new ScenarioBatchEngine(
            new List<IScenarioBatchStep>
            {
                new TieredScenarioBatchAdjuster(new List<double> { 2420.0, 4830.0 }, new List<double> { 0.97, 1.09 }),
                new RoundingScenarioBatchAdjuster(1.0),
                new LabelScenarioBatchAdjuster(new Dictionary<string, double> { ["east"] = 0.82, ["online"] = 1.32 }),
                new PercentScenarioBatchAdjuster(21.3),
            },
            new List<string>(),
            3.3,
            35730.2);
    }

    public double Run(ScenarioBatch item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioBatch> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
