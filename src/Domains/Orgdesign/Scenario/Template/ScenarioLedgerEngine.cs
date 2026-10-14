namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario ledger calculation.</summary>
public interface IScenarioLedgerStep
{
    string Describe();
    double Adjust(ScenarioLedger item, double valueValue);
}

public sealed record TitleScenarioLedgerAdjuster(Dictionary<string, double> Factors) : IScenarioLedgerStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(ScenarioLedger item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record ScenarioLedgerBatchSizeOffsetAdjuster(double Weight) : IScenarioLedgerStep
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(ScenarioLedger item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record CapScenarioLedgerAdjuster(double Cap) : IScenarioLedgerStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioLedger item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingScenarioLedgerAdjuster(double Step) : IScenarioLedgerStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ScenarioLedger item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the scenario ledger steps in order and keeps a trace of each.</summary>
public sealed class ScenarioLedgerEngine(List<IScenarioLedgerStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioLedgerStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioLedgerEngine DefaultScenarioLedgerEngine()
    {
        return new ScenarioLedgerEngine(
            new List<IScenarioLedgerStep>
            {
                new TitleScenarioLedgerAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.99, ["north"] = 1.04, ["online"] = 1.4 }),
                new ScenarioLedgerBatchSizeOffsetAdjuster(1.19),
                new CapScenarioLedgerAdjuster(47240.0),
                new RoundingScenarioLedgerAdjuster(0.5),
            },
            new List<string>(),
            4.3,
            87818.3);
    }

    public double Price(ScenarioLedger item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioLedger> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
