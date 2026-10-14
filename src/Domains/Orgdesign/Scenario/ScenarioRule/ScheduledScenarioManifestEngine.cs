namespace Acme.Workforce.Orgdesign.Scenario.ScenarioRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the scheduled scenario manifest calculation.</summary>
public interface IScheduledScenarioManifestFactor
{
    string Describe();
    double Adjust(ScheduledScenarioManifest item, double valueValue);
}

public sealed record FlaggedScheduledScenarioManifestAdjuster(Registry Flags, string Flag, double Percent) : IScheduledScenarioManifestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(ScheduledScenarioManifest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapScheduledScenarioManifestAdjuster(double Cap) : IScheduledScenarioManifestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ScheduledScenarioManifest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ScheduledScenarioManifestMarginOffsetAdjuster(double Weight) : IScheduledScenarioManifestFactor
{
    public string Describe()
    {
        return $"plus margin times {Weight}";
    }

    public double Adjust(ScheduledScenarioManifest item, double valueValue)
    {
        return valueValue + item.Margin * Weight;
    }
}

public sealed record MinimumScheduledScenarioManifestAdjuster(double Minimum) : IScheduledScenarioManifestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ScheduledScenarioManifest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredScheduledScenarioManifestAdjuster(List<double> Bounds, List<double> Rates) : IScheduledScenarioManifestFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ScheduledScenarioManifest item, double valueValue)
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

/// <summary>Runs the scheduled scenario manifest steps in order and keeps a trace of each.</summary>
public sealed class ScheduledScenarioManifestEngine(List<IScheduledScenarioManifestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IScheduledScenarioManifestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ScheduledScenarioManifestEngine DefaultScheduledScenarioManifestEngine(Registry flags)
    {
        return new ScheduledScenarioManifestEngine(
            new List<IScheduledScenarioManifestFactor>
            {
                new FlaggedScheduledScenarioManifestAdjuster(flags, "scheduled-scenario-manifest-seasonal", 7.3),
                new CapScheduledScenarioManifestAdjuster(853.0),
                new ScheduledScenarioManifestMarginOffsetAdjuster(0.76),
                new MinimumScheduledScenarioManifestAdjuster(7.4),
                new TieredScheduledScenarioManifestAdjuster(
                    new List<double> { 2300.0, 5200.0, 5485.0, 7133.0 },
                    new List<double> { 1.11, 1.11, 0.86, 1.15 }),
            },
            new List<string>(),
            2.9,
            57192.1);
    }

    public double Evaluate(ScheduledScenarioManifest item)
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

    public Dictionary<string, double> EvaluateAll(List<ScheduledScenarioManifest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
