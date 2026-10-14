namespace Acme.Workforce.Talent.SuccessionPlan.Entry;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal succession plan preference calculation.</summary>
public interface ISeasonalSuccessionPlanPreferenceModifier
{
    string Describe();
    double Adjust(SeasonalSuccessionPlanPreference item, double valueValue);
}

public sealed record FlaggedSeasonalSuccessionPlanPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalSuccessionPlanPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalSuccessionPlanPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSeasonalSuccessionPlanPreferenceAdjuster(double Percent) : ISeasonalSuccessionPlanPreferenceModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SeasonalSuccessionPlanPreference item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SeasonalSuccessionPlanPreferenceStartsAtOffsetAdjuster(double Weight) : ISeasonalSuccessionPlanPreferenceModifier
{
    public string Describe()
    {
        return $"plus starts at times {Weight}";
    }

    public double Adjust(SeasonalSuccessionPlanPreference item, double valueValue)
    {
        return valueValue + (double)item.StartsAt * Weight;
    }
}

/// <summary>Runs the seasonal succession plan preference steps in order and keeps a trace of each.</summary>
public sealed class SeasonalSuccessionPlanPreferenceEngine(List<ISeasonalSuccessionPlanPreferenceModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalSuccessionPlanPreferenceModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalSuccessionPlanPreferenceEngine DefaultSeasonalSuccessionPlanPreferenceEngine(Registry flags)
    {
        return new SeasonalSuccessionPlanPreferenceEngine(
            new List<ISeasonalSuccessionPlanPreferenceModifier>
            {
                new FlaggedSeasonalSuccessionPlanPreferenceAdjuster(
                    flags,
                    "seasonal-succession-plan-preference-manual",
                    3.6),
                new PercentSeasonalSuccessionPlanPreferenceAdjuster(10.3),
                new SeasonalSuccessionPlanPreferenceStartsAtOffsetAdjuster(0.78),
            },
            new List<string>(),
            3.7,
            18725.8);
    }

    public double Evaluate(SeasonalSuccessionPlanPreference item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalSuccessionPlanPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
