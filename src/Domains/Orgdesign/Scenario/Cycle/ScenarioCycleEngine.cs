namespace Acme.Workforce.Orgdesign.Scenario.Cycle;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario cycle calculation.</summary>
public interface IScenarioCycleAdjuster
{
    string Describe();
    double Adjust(ScenarioCycle item, double valueValue);
}

public sealed record PercentScenarioCycleAdjuster(double Percent) : IScenarioCycleAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioCycle item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryScenarioCycleAdjuster(Dictionary<string, double> Factors) : IScenarioCycleAdjuster
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ScenarioCycle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapScenarioCycleAdjuster(double Cap) : IScenarioCycleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioCycle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScenarioCycleAdjuster(double Minimum) : IScenarioCycleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioCycle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the scenario cycle steps in order and keeps a trace of each.</summary>
public sealed class ScenarioCycleEngine(List<IScenarioCycleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioCycleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioCycleEngine DefaultScenarioCycleEngine()
    {
        return new ScenarioCycleEngine(
            new List<IScenarioCycleAdjuster>
            {
                new PercentScenarioCycleAdjuster(-11.7),
                new CountryScenarioCycleAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.07, ["north"] = 1.27 }),
                new CapScenarioCycleAdjuster(16104.0),
                new MinimumScenarioCycleAdjuster(37.3),
            },
            new List<string>(),
            1.7,
            37030.6);
    }

    public double Evaluate(ScenarioCycle item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioCycle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
