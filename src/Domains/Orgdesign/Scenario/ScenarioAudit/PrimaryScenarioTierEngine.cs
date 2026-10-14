namespace Acme.Workforce.Orgdesign.Scenario.ScenarioAudit;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary scenario tier calculation.</summary>
public interface IPrimaryScenarioTierModifier
{
    string Describe();
    double Adjust(PrimaryScenarioTier item, double valueValue);
}

public sealed record PercentPrimaryScenarioTierAdjuster(double Percent) : IPrimaryScenarioTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryScenarioTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TitlePrimaryScenarioTierAdjuster(Dictionary<string, double> Factors) : IPrimaryScenarioTierModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PrimaryScenarioTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PrimaryScenarioTierWindowDaysOffsetAdjuster(double Weight) : IPrimaryScenarioTierModifier
{
    public string Describe()
    {
        return $"plus window days times {Weight}";
    }

    public double Adjust(PrimaryScenarioTier item, double valueValue)
    {
        return valueValue + (double)item.WindowDays * Weight;
    }
}

public sealed record RoundingPrimaryScenarioTierAdjuster(double Step) : IPrimaryScenarioTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryScenarioTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the primary scenario tier steps in order and keeps a trace of each.</summary>
public sealed class PrimaryScenarioTierEngine(List<IPrimaryScenarioTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryScenarioTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryScenarioTierEngine DefaultPrimaryScenarioTierEngine()
    {
        return new PrimaryScenarioTierEngine(
            new List<IPrimaryScenarioTierModifier>
            {
                new PercentPrimaryScenarioTierAdjuster(12.8),
                new TitlePrimaryScenarioTierAdjuster(new Dictionary<string, double> { ["east"] = 0.88 }),
                new PrimaryScenarioTierWindowDaysOffsetAdjuster(1.91),
                new RoundingPrimaryScenarioTierAdjuster(0.05),
            },
            new List<string>(),
            2.2,
            74345.9);
    }

    public double Evaluate(PrimaryScenarioTier item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryScenarioTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
