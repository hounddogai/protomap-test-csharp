namespace Acme.Workforce.Orgdesign.CostModel.Preference;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the seasonal cost model preference calculation.</summary>
public interface ISeasonalCostModelPreferenceAdjuster
{
    string Describe();
    double Adjust(SeasonalCostModelPreference item, double valueValue);
}

public sealed record MinimumSeasonalCostModelPreferenceAdjuster(double Minimum) : ISeasonalCostModelPreferenceAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SeasonalCostModelPreference item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredSeasonalCostModelPreferenceAdjuster(List<double> Bounds, List<double> Rates) : ISeasonalCostModelPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SeasonalCostModelPreference item, double valueValue)
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

public sealed record FlaggedSeasonalCostModelPreferenceAdjuster(Registry Flags, string Flag, double Percent) : ISeasonalCostModelPreferenceAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SeasonalCostModelPreference item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the seasonal cost model preference steps in order and keeps a trace of each.</summary>
public sealed class SeasonalCostModelPreferenceEngine(List<ISeasonalCostModelPreferenceAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISeasonalCostModelPreferenceAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SeasonalCostModelPreferenceEngine DefaultSeasonalCostModelPreferenceEngine(Registry flags)
    {
        return new SeasonalCostModelPreferenceEngine(
            new List<ISeasonalCostModelPreferenceAdjuster>
            {
                new MinimumSeasonalCostModelPreferenceAdjuster(18.3),
                new TieredSeasonalCostModelPreferenceAdjuster(
                    new List<double> { 1242.0, 2853.0, 5575.0 },
                    new List<double> { 1.08, 1.16, 1.12 }),
                new FlaggedSeasonalCostModelPreferenceAdjuster(
                    flags,
                    "seasonal-cost-model-preference-scheduled",
                    1.2),
            },
            new List<string>(),
            1.6,
            52664.0);
    }

    public double Run(SeasonalCostModelPreference item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<SeasonalCostModelPreference> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
