namespace Acme.Workforce.Orgdesign.Scenario.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global scenario policy calculation.</summary>
public interface IGlobalScenarioPolicyModifier
{
    string Describe();
    double Adjust(GlobalScenarioPolicy item, double valueValue);
}

public sealed record RoundingGlobalScenarioPolicyAdjuster(double Step) : IGlobalScenarioPolicyModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalScenarioPolicy item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapGlobalScenarioPolicyAdjuster(double Cap) : IGlobalScenarioPolicyModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalScenarioPolicy item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentGlobalScenarioPolicyAdjuster(double Percent) : IGlobalScenarioPolicyModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(GlobalScenarioPolicy item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredGlobalScenarioPolicyAdjuster(List<double> Bounds, List<double> Rates) : IGlobalScenarioPolicyModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(GlobalScenarioPolicy item, double valueValue)
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

public sealed record FlaggedGlobalScenarioPolicyAdjuster(Registry Flags, string Flag, double Percent) : IGlobalScenarioPolicyModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(GlobalScenarioPolicy item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the global scenario policy steps in order and keeps a trace of each.</summary>
public sealed class GlobalScenarioPolicyEngine(List<IGlobalScenarioPolicyModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalScenarioPolicyModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalScenarioPolicyEngine DefaultGlobalScenarioPolicyEngine(Registry flags)
    {
        return new GlobalScenarioPolicyEngine(
            new List<IGlobalScenarioPolicyModifier>
            {
                new RoundingGlobalScenarioPolicyAdjuster(0.5),
                new CapGlobalScenarioPolicyAdjuster(40938.0),
                new PercentGlobalScenarioPolicyAdjuster(12.7),
                new TieredGlobalScenarioPolicyAdjuster(
                    new List<double> { 1061.0, 2836.0, 7090.0, 8286.0 },
                    new List<double> { 0.88, 1.2, 0.83, 0.94 }),
                new FlaggedGlobalScenarioPolicyAdjuster(flags, "global-scenario-policy-pending", 9.3),
            },
            new List<string>(),
            1.5,
            79972.6);
    }

    public double Compute(GlobalScenarioPolicy item)
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

    public Dictionary<string, double> EvaluateAll(List<GlobalScenarioPolicy> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
