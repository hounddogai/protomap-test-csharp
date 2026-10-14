namespace Acme.Workforce.Orgdesign.Scenario.ScenarioEstimate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global scenario bundle calculation.</summary>
public interface IGlobalScenarioBundleStep
{
    string Describe();
    double Adjust(GlobalScenarioBundle item, double valueValue);
}

public sealed record MinimumGlobalScenarioBundleAdjuster(double Minimum) : IGlobalScenarioBundleStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalScenarioBundle item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CountryGlobalScenarioBundleAdjuster(Dictionary<string, double> Factors) : IGlobalScenarioBundleStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(GlobalScenarioBundle item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapGlobalScenarioBundleAdjuster(double Cap) : IGlobalScenarioBundleStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalScenarioBundle item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global scenario bundle steps in order and keeps a trace of each.</summary>
public sealed class GlobalScenarioBundleEngine(List<IGlobalScenarioBundleStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalScenarioBundleStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalScenarioBundleEngine DefaultGlobalScenarioBundleEngine()
    {
        return new GlobalScenarioBundleEngine(
            new List<IGlobalScenarioBundleStep>
            {
                new MinimumGlobalScenarioBundleAdjuster(34.4),
                new CountryGlobalScenarioBundleAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.37, ["east"] = 1.24 }),
                new CapGlobalScenarioBundleAdjuster(47379.0),
            },
            new List<string>(),
            3.9,
            22578.8);
    }

    public double Compute(GlobalScenarioBundle item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalScenarioBundle> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
