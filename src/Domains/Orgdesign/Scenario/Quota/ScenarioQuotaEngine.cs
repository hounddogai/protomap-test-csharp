namespace Acme.Workforce.Orgdesign.Scenario.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario quota calculation.</summary>
public interface IScenarioQuotaModifier
{
    string Describe();
    double Adjust(ScenarioQuota item, double valueValue);
}

public sealed record MinimumScenarioQuotaAdjuster(double Minimum) : IScenarioQuotaModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapScenarioQuotaAdjuster(double Cap) : IScenarioQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CountryScenarioQuotaAdjuster(Dictionary<string, double> Factors) : IScenarioQuotaModifier
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(ScenarioQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the scenario quota steps in order and keeps a trace of each.</summary>
public sealed class ScenarioQuotaEngine(List<IScenarioQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioQuotaEngine DefaultScenarioQuotaEngine()
    {
        return new ScenarioQuotaEngine(
            new List<IScenarioQuotaModifier>
            {
                new MinimumScenarioQuotaAdjuster(30.9),
                new CapScenarioQuotaAdjuster(49801.0),
                new CountryScenarioQuotaAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.31, ["online"] = 1.14 }),
            },
            new List<string>(),
            1.5,
            77785.7);
    }

    public double Run(ScenarioQuota item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
