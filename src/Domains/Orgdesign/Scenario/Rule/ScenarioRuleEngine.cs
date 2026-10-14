namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario rule calculation.</summary>
public interface IScenarioRuleAdjuster
{
    string Describe();
    double Adjust(ScenarioRule item, double valueValue);
}

public sealed record MinimumScenarioRuleAdjuster(double Minimum) : IScenarioRuleAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioRule item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScenarioRuleAdjuster(double Cap) : IScenarioRuleAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioRule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScenarioRuleAdjuster(double Step) : IScenarioRuleAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioRule item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scenario rule steps in order and keeps a trace of each.</summary>
public sealed class ScenarioRuleEngine(List<IScenarioRuleAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioRuleAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioRuleEngine DefaultScenarioRuleEngine()
    {
        return new ScenarioRuleEngine(
            new List<IScenarioRuleAdjuster>
            {
                new MinimumScenarioRuleAdjuster(40.5),
                new CapScenarioRuleAdjuster(17686.0),
                new RoundingScenarioRuleAdjuster(1.0),
            },
            new List<string>(),
            0.4,
            79186.5);
    }

    public double Run(ScenarioRule item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioRule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
