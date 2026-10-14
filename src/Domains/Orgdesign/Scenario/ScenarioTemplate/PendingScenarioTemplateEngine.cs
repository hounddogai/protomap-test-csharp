namespace Acme.Workforce.Orgdesign.Scenario.ScenarioTemplate;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending scenario template calculation.</summary>
public interface IPendingScenarioTemplateStep
{
    string Describe();
    double Adjust(PendingScenarioTemplate item, double valueValue);
}

public sealed record TieredPendingScenarioTemplateAdjuster(List<double> Bounds, List<double> Rates) : IPendingScenarioTemplateStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingScenarioTemplate item, double valueValue)
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

public sealed record PendingScenarioTemplateEndsAtOffsetAdjuster(double Weight) : IPendingScenarioTemplateStep
{
    public string Describe()
    {
        return $"plus ends at times {Weight}";
    }

    public double Adjust(PendingScenarioTemplate item, double valueValue)
    {
        return valueValue + (double)item.EndsAt * Weight;
    }
}

public sealed record UnitPendingScenarioTemplateAdjuster(Dictionary<string, double> Factors) : IPendingScenarioTemplateStep
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(PendingScenarioTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumPendingScenarioTemplateAdjuster(double Minimum) : IPendingScenarioTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingScenarioTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPendingScenarioTemplateAdjuster(double Step) : IPendingScenarioTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingScenarioTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the pending scenario template steps in order and keeps a trace of each.</summary>
public sealed class PendingScenarioTemplateEngine(List<IPendingScenarioTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingScenarioTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingScenarioTemplateEngine DefaultPendingScenarioTemplateEngine()
    {
        return new PendingScenarioTemplateEngine(
            new List<IPendingScenarioTemplateStep>
            {
                new TieredPendingScenarioTemplateAdjuster(
                    new List<double> { 4784.0, 5235.0, 7136.0 },
                    new List<double> { 0.85, 1.01, 0.86 }),
                new PendingScenarioTemplateEndsAtOffsetAdjuster(0.07),
                new UnitPendingScenarioTemplateAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.97, ["west"] = 1.4 }),
                new MinimumPendingScenarioTemplateAdjuster(30.7),
                new RoundingPendingScenarioTemplateAdjuster(1.0),
            },
            new List<string>(),
            2.6,
            58581.1);
    }

    public double Run(PendingScenarioTemplate item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<PendingScenarioTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
