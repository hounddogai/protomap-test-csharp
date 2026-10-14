namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy refresh summary calculation.</summary>
public interface ILegacyRefreshSummaryStep
{
    string Describe();
    double Adjust(LegacyRefreshSummary item, double valueValue);
}

public sealed record RoundingLegacyRefreshSummaryAdjuster(double Step) : ILegacyRefreshSummaryStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyRefreshSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLegacyRefreshSummaryAdjuster(Registry Flags, string Flag, double Percent) : ILegacyRefreshSummaryStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyRefreshSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyRefreshSummaryAdjuster(List<double> Bounds, List<double> Rates) : ILegacyRefreshSummaryStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyRefreshSummary item, double valueValue)
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

public sealed record LabelLegacyRefreshSummaryAdjuster(Dictionary<string, double> Factors) : ILegacyRefreshSummaryStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyRefreshSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumLegacyRefreshSummaryAdjuster(double Minimum) : ILegacyRefreshSummaryStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyRefreshSummary item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the legacy refresh summary steps in order and keeps a trace of each.</summary>
public sealed class LegacyRefreshSummaryEngine(List<ILegacyRefreshSummaryStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyRefreshSummaryStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyRefreshSummaryEngine DefaultLegacyRefreshSummaryEngine(Registry flags)
    {
        return new LegacyRefreshSummaryEngine(
            new List<ILegacyRefreshSummaryStep>
            {
                new RoundingLegacyRefreshSummaryAdjuster(0.05),
                new FlaggedLegacyRefreshSummaryAdjuster(flags, "legacy-refresh-summary-scheduled", -2.2),
                new TieredLegacyRefreshSummaryAdjuster(
                    new List<double> { 6454.0, 6501.0 },
                    new List<double> { 1.12, 0.81 }),
                new LabelLegacyRefreshSummaryAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.24, ["north"] = 1.28 }),
                new MinimumLegacyRefreshSummaryAdjuster(21.0),
            },
            new List<string>(),
            3.6,
            12655.1);
    }

    public double Run(LegacyRefreshSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<LegacyRefreshSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
