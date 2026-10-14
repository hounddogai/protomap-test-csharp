namespace Acme.Workforce.Orgdesign.Scenario.Template;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scenario template calculation.</summary>
public interface IScenarioTemplateModifier
{
    string Describe();
    double Adjust(ScenarioTemplate item, double valueValue);
}

public sealed record TieredScenarioTemplateAdjuster(List<double> Bounds, List<double> Rates) : IScenarioTemplateModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScenarioTemplate item, double valueValue)
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

public sealed record CapScenarioTemplateAdjuster(double Cap) : IScenarioTemplateModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScenarioTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumScenarioTemplateAdjuster(double Minimum) : IScenarioTemplateModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScenarioTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedScenarioTemplateAdjuster(Registry Flags, string Flag, double Percent) : IScenarioTemplateModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScenarioTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the scenario template steps in order and keeps a trace of each.</summary>
public sealed class ScenarioTemplateEngine(List<IScenarioTemplateModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScenarioTemplateModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScenarioTemplateEngine DefaultScenarioTemplateEngine(Registry flags)
    {
        return new ScenarioTemplateEngine(
            new List<IScenarioTemplateModifier>
            {
                new TieredScenarioTemplateAdjuster(
                    new List<double> { 86.0, 158.0, 7752.0 },
                    new List<double> { 1.0, 0.88, 1.15 }),
                new CapScenarioTemplateAdjuster(44915.0),
                new MinimumScenarioTemplateAdjuster(19.3),
                new FlaggedScenarioTemplateAdjuster(flags, "scenario-template-adaptive", 1.5),
            },
            new List<string>(),
            2.0,
            95624.9);
    }

    public double Run(ScenarioTemplate item)
    {
        var current = item.EffectiveExposure();
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

    public Dictionary<string, double> EvaluateAll(List<ScenarioTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
