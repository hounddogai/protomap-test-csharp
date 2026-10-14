namespace Acme.Workforce.Orgdesign.Scenario.WeightedScenarioTemplate;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the adaptive scenario template calculation.</summary>
public interface IAdaptiveScenarioTemplateStep
{
    string Describe();
    double Adjust(AdaptiveScenarioTemplate item, double valueValue);
}

public sealed record MinimumAdaptiveScenarioTemplateAdjuster(double Minimum) : IAdaptiveScenarioTemplateStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AdaptiveScenarioTemplate item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record AdaptiveScenarioTemplateSequenceOffsetAdjuster(double Weight) : IAdaptiveScenarioTemplateStep
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(AdaptiveScenarioTemplate item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedAdaptiveScenarioTemplateAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveScenarioTemplateStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveScenarioTemplate item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record NameAdaptiveScenarioTemplateAdjuster(Dictionary<string, double> Factors) : IAdaptiveScenarioTemplateStep
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(AdaptiveScenarioTemplate item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapAdaptiveScenarioTemplateAdjuster(double Cap) : IAdaptiveScenarioTemplateStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveScenarioTemplate item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAdaptiveScenarioTemplateAdjuster(double Step) : IAdaptiveScenarioTemplateStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AdaptiveScenarioTemplate item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the adaptive scenario template steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveScenarioTemplateEngine(List<IAdaptiveScenarioTemplateStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveScenarioTemplateStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveScenarioTemplateEngine DefaultAdaptiveScenarioTemplateEngine(Registry flags)
    {
        return new AdaptiveScenarioTemplateEngine(
            new List<IAdaptiveScenarioTemplateStep>
            {
                new MinimumAdaptiveScenarioTemplateAdjuster(4.2),
                new AdaptiveScenarioTemplateSequenceOffsetAdjuster(0.87),
                new FlaggedAdaptiveScenarioTemplateAdjuster(flags, "adaptive-scenario-template-fallback", -9.7),
                new NameAdaptiveScenarioTemplateAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.97, ["east"] = 1.4 }),
                new CapAdaptiveScenarioTemplateAdjuster(35187.0),
                new RoundingAdaptiveScenarioTemplateAdjuster(0.5),
            },
            new List<string>(),
            1.2,
            38128.9);
    }

    public double Price(AdaptiveScenarioTemplate item)
    {
        var current = item.AdjustedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveScenarioTemplate> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
