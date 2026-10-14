namespace Acme.Workforce.Orgdesign.Scenario.Audit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario audit calculation.</summary>
public interface IScenarioAuditFactor
{
    string Describe();
    double Adjust(ScenarioAudit item, double valueValue);
}

public sealed record CapScenarioAuditAdjuster(double Cap) : IScenarioAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScenarioAuditAdjuster(double Minimum) : IScenarioAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryScenarioAuditAdjuster(Dictionary<string, double> Factors) : IScenarioAuditFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(ScenarioAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scenario audit steps in order and keeps a trace of each.</summary>
public sealed class ScenarioAuditEngine(List<IScenarioAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioAuditEngine DefaultScenarioAuditEngine()
    {
        return new ScenarioAuditEngine(
            new List<IScenarioAuditFactor>
            {
                new CapScenarioAuditAdjuster(28389.0),
                new MinimumScenarioAuditAdjuster(18.6),
                new CategoryScenarioAuditAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.95, ["retail"] = 1.24 }),
            },
            new List<string>(),
            0.2,
            74264.7);
    }

    public double Evaluate(ScenarioAudit item)
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
