namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario bundle calculation.</summary>
public interface IScenarioBundleAdjuster
{
    string Describe();
    double Adjust(ScenarioBundle item, double valueValue);
}

public sealed record TieredScenarioBundleAdjuster(List<double> Bounds, List<double> Rates) : IScenarioBundleAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioBundle item, double valueValue)
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

public sealed record PercentScenarioBundleAdjuster(double Percent) : IScenarioBundleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioBundle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScenarioBundleAdjuster(double Cap) : IScenarioBundleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScenarioBundleAdjuster(double Step) : IScenarioBundleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioBundle item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CountryScenarioBundleAdjuster(Dictionary<string, double> Factors) : IScenarioBundleAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ScenarioBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scenario bundle steps in order and keeps a trace of each.</summary>
public sealed class ScenarioBundleEngine(List<IScenarioBundleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioBundleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioBundleEngine DefaultScenarioBundleEngine()
    {
        return new ScenarioBundleEngine(
            new List<IScenarioBundleAdjuster>
            {
                new TieredScenarioBundleAdjuster(
                    new List<double> { 2440.0, 3010.0, 8623.0 },
                    new List<double> { 1.09, 1.2, 1.09 }),
                new PercentScenarioBundleAdjuster(23.2),
                new CapScenarioBundleAdjuster(6580.0),
                new RoundingScenarioBundleAdjuster(0.5),
                new CountryScenarioBundleAdjuster(new Dictionary<string, double> { ["central"] = 1.19 }),
            },
            new List<string>(),
            2.3,
            82014.1);
    }

    public double Price(ScenarioBundle item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
