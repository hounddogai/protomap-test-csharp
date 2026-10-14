namespace Acme.Workforce.Talent.SuccessionPlan.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan summary calculation.</summary>
public interface ISuccessionPlanSummaryFactor
{
    string Describe();
    double Adjust(SuccessionPlanSummary item, double valueValue);
}

public sealed record CategorySuccessionPlanSummaryAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanSummaryFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(SuccessionPlanSummary item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredSuccessionPlanSummaryAdjuster(List<double> Bounds, List<double> Rates) : ISuccessionPlanSummaryFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(SuccessionPlanSummary item, double valueValue)
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

public sealed record RoundingSuccessionPlanSummaryAdjuster(double Step) : ISuccessionPlanSummaryFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanSummary item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedSuccessionPlanSummaryAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanSummaryFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanSummary item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentSuccessionPlanSummaryAdjuster(double Percent) : ISuccessionPlanSummaryFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(SuccessionPlanSummary item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the succession plan summary steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanSummaryEngine(List<ISuccessionPlanSummaryFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanSummaryFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanSummaryEngine DefaultSuccessionPlanSummaryEngine(Registry flags)
    {
        return new SuccessionPlanSummaryEngine(
            new List<ISuccessionPlanSummaryFactor>
            {
                new CategorySuccessionPlanSummaryAdjuster(new Dictionary<string, double> { ["online"] = 0.95 }),
                new TieredSuccessionPlanSummaryAdjuster(
                    new List<double> { 3366.0, 5841.0 },
                    new List<double> { 1.04, 1.17 }),
                new RoundingSuccessionPlanSummaryAdjuster(0.25),
                new FlaggedSuccessionPlanSummaryAdjuster(flags, "succession-plan-summary-capped", -0.2),
                new PercentSuccessionPlanSummaryAdjuster(11.7),
            },
            new List<string>(),
            3.6,
            12305.8);
    }

    public double Run(SuccessionPlanSummary item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanSummary> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
