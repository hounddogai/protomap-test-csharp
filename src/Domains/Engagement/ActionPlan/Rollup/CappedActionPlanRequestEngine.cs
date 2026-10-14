namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped action plan request calculation.</summary>
public interface ICappedActionPlanRequestStep
{
    string Describe();
    double Adjust(CappedActionPlanRequest item, double valueValue);
}

public sealed record TieredCappedActionPlanRequestAdjuster(List<double> Bounds, List<double> Rates) : ICappedActionPlanRequestStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(CappedActionPlanRequest item, double valueValue)
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

public sealed record FlaggedCappedActionPlanRequestAdjuster(Registry Flags, string Flag, double Percent) : ICappedActionPlanRequestStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedActionPlanRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedActionPlanRequestAdjuster(double Step) : ICappedActionPlanRequestStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedActionPlanRequest item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the capped action plan request steps in order and keeps a trace of each.</summary>
public sealed class CappedActionPlanRequestEngine(List<ICappedActionPlanRequestStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedActionPlanRequestStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedActionPlanRequestEngine DefaultCappedActionPlanRequestEngine(Registry flags)
    {
        return new CappedActionPlanRequestEngine(
            new List<ICappedActionPlanRequestStep>
            {
                new TieredCappedActionPlanRequestAdjuster(
                    new List<double> { 1642.0, 2696.0, 9074.0 },
                    new List<double> { 0.83, 1.05, 1.08 }),
                new FlaggedCappedActionPlanRequestAdjuster(flags, "capped-action-plan-request-tiered", -1.2),
                new RoundingCappedActionPlanRequestAdjuster(0.1),
            },
            new List<string>(),
            0.7,
            41480.8);
    }

    public double Price(CappedActionPlanRequest item)
    {
        var current = item.WeightedVolume();
        Trace = new List<string>();
        for (var index = 0; index < Steps.Count; index++)
        {
            var step = Steps[index];
            var before = current;
            current = step.Adjust(item, current);
            if (before != current)
            {
                Trace.Add($"{index + 1}. {step.Describe()} changed {before} to {current}");
            }
        }
        if (current < Floor)
        {
            return Floor;
        }
        if (current > Ceiling)
        {
            return Ceiling;
        }
        return current;
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<CappedActionPlanRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
