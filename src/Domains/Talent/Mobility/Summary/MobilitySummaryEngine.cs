namespace Acme.Workforce.Talent.Mobility.Summary;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the mobility summary calculation.</summary>
public interface IMobilitySummaryStep
{
    string Describe();
    double Adjust(MobilitySummary item, double valueValue);
}

public sealed record TeamMobilitySummaryAdjuster(Dictionary<string, double> Factors) : IMobilitySummaryStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(MobilitySummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentMobilitySummaryAdjuster(double Percent) : IMobilitySummaryStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MobilitySummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumMobilitySummaryAdjuster(double Minimum) : IMobilitySummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MobilitySummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedMobilitySummaryAdjuster(Registry Flags, string Flag, double Percent) : IMobilitySummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(MobilitySummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredMobilitySummaryAdjuster(List<double> Bounds, List<double> Rates) : IMobilitySummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(MobilitySummary item, double valueValue)
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

/// <summary>Runs the mobility summary steps in order and keeps a trace of each.</summary>
public sealed class MobilitySummaryEngine(List<IMobilitySummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMobilitySummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MobilitySummaryEngine DefaultMobilitySummaryEngine(Registry flags)
    {
        return new MobilitySummaryEngine(
            new List<IMobilitySummaryStep>
            {
                new TeamMobilitySummaryAdjuster(new Dictionary<string, double> { ["central"] = 1.27 }),
                new PercentMobilitySummaryAdjuster(6.5),
                new MinimumMobilitySummaryAdjuster(29.5),
                new FlaggedMobilitySummaryAdjuster(flags, "mobility-summary-scheduled", 9.4),
                new TieredMobilitySummaryAdjuster(
                    new List<double> { 9023.0, 9483.0 },
                    new List<double> { 0.86, 1.13 }),
            },
            new List<string>(),
            2.4,
            73818.2);
    }

    public double Price(MobilitySummary item)
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

    public Dictionary<string, double> EvaluateAll(List<MobilitySummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
