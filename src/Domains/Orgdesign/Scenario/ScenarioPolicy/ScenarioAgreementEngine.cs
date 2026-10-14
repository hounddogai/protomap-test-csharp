namespace Acme.Workforce.Orgdesign.Scenario.ScenarioPolicy;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario agreement calculation.</summary>
public interface IScenarioAgreementFactor
{
    string Describe();
    double Adjust(ScenarioAgreement item, double valueValue);
}

public sealed record MinimumScenarioAgreementAdjuster(double Minimum) : IScenarioAgreementFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioAgreement item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ScenarioAgreementBatchSizeOffsetAdjuster(double Weight) : IScenarioAgreementFactor
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(ScenarioAgreement item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentScenarioAgreementAdjuster(double Percent) : IScenarioAgreementFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ScenarioAgreement item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scenario agreement steps in order and keeps a trace of each.</summary>
public sealed class ScenarioAgreementEngine(List<IScenarioAgreementFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioAgreementFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioAgreementEngine DefaultScenarioAgreementEngine()
    {
        return new ScenarioAgreementEngine(
            new List<IScenarioAgreementFactor>
            {
                new MinimumScenarioAgreementAdjuster(43.9),
                new ScenarioAgreementBatchSizeOffsetAdjuster(1.44),
                new PercentScenarioAgreementAdjuster(-10.3),
            },
            new List<string>(),
            3.9,
            99823.1);
    }

    public double Evaluate(ScenarioAgreement item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioAgreement> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
