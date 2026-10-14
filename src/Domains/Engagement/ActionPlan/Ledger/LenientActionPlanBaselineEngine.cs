namespace Acme.Workforce.Engagement.ActionPlan.Ledger;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient action plan baseline calculation.</summary>
public interface ILenientActionPlanBaselineFactor
{
    string Describe();
    double Adjust(LenientActionPlanBaseline item, double valueValue);
}

public sealed record LabelLenientActionPlanBaselineAdjuster(Dictionary<string, double> Factors) : ILenientActionPlanBaselineFactor
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientActionPlanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientActionPlanBaselineAdjuster(Registry Flags, string Flag, double Percent) : ILenientActionPlanBaselineFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientActionPlanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLenientActionPlanBaselineAdjuster(List<double> Bounds, List<double> Rates) : ILenientActionPlanBaselineFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientActionPlanBaseline item, double valueValue)
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

/// <summary>Runs the lenient action plan baseline steps in order and keeps a trace of each.</summary>
public sealed class LenientActionPlanBaselineEngine(List<ILenientActionPlanBaselineFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientActionPlanBaselineFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientActionPlanBaselineEngine DefaultLenientActionPlanBaselineEngine(Registry flags)
    {
        return new LenientActionPlanBaselineEngine(
            new List<ILenientActionPlanBaselineFactor>
            {
                new LabelLenientActionPlanBaselineAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.71, ["south"] = 0.94 }),
                new FlaggedLenientActionPlanBaselineAdjuster(flags, "lenient-action-plan-baseline-fallback", -0.2),
                new TieredLenientActionPlanBaselineAdjuster(
                    new List<double> { 896.0, 1771.0, 5859.0 },
                    new List<double> { 0.82, 0.95, 1.16 }),
            },
            new List<string>(),
            4.2,
            46938.1);
    }

    public double Price(LenientActionPlanBaseline item)
    {
        var current = item.EffectiveLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientActionPlanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
