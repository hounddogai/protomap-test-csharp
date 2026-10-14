namespace Acme.Workforce.Orgdesign.Scenario.Rule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending scenario request calculation.</summary>
public interface IPendingScenarioRequestFactor
{
    string Describe();
    double Adjust(PendingScenarioRequest item, double valueValue);
}

public sealed record PercentPendingScenarioRequestAdjuster(double Percent) : IPendingScenarioRequestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingScenarioRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPendingScenarioRequestAdjuster(double Minimum) : IPendingScenarioRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingScenarioRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingScenarioRequestAdjuster(List<double> Bounds, List<double> Rates) : IPendingScenarioRequestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingScenarioRequest item, double valueValue)
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

/// <summary>Runs the pending scenario request steps in order and keeps a trace of each.</summary>
public sealed class PendingScenarioRequestEngine(List<IPendingScenarioRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingScenarioRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingScenarioRequestEngine DefaultPendingScenarioRequestEngine()
    {
        return new PendingScenarioRequestEngine(
            new List<IPendingScenarioRequestFactor>
            {
                new PercentPendingScenarioRequestAdjuster(-15.5),
                new MinimumPendingScenarioRequestAdjuster(9.7),
                new TieredPendingScenarioRequestAdjuster(
                    new List<double> { 7566.0, 7979.0 },
                    new List<double> { 0.87, 0.99 }),
            },
            new List<string>(),
            4.6,
            79518.2);
    }

    public double Run(PendingScenarioRequest item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingScenarioRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
