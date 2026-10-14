namespace Acme.Workforce.Engagement.ActionPlan.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the fallback action plan draft calculation.</summary>
public interface IFallbackActionPlanDraftStep
{
    string Describe();
    double Adjust(FallbackActionPlanDraft item, double valueValue);
}

public sealed record LabelFallbackActionPlanDraftAdjuster(Dictionary<string, double> Factors) : IFallbackActionPlanDraftStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(FallbackActionPlanDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FallbackActionPlanDraftAmountOffsetAdjuster(double Weight) : IFallbackActionPlanDraftStep
{
    public string Describe()
    {
        return $"plus amount times {Weight}";
    }

    public double Adjust(FallbackActionPlanDraft item, double valueValue)
    {
        return valueValue + item.Amount * Weight;
    }
}

public sealed record RoundingFallbackActionPlanDraftAdjuster(double Step) : IFallbackActionPlanDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(FallbackActionPlanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapFallbackActionPlanDraftAdjuster(double Cap) : IFallbackActionPlanDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(FallbackActionPlanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredFallbackActionPlanDraftAdjuster(List<double> Bounds, List<double> Rates) : IFallbackActionPlanDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(FallbackActionPlanDraft item, double valueValue)
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

/// <summary>Runs the fallback action plan draft steps in order and keeps a trace of each.</summary>
public sealed class FallbackActionPlanDraftEngine(List<IFallbackActionPlanDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IFallbackActionPlanDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static FallbackActionPlanDraftEngine DefaultFallbackActionPlanDraftEngine()
    {
        return new FallbackActionPlanDraftEngine(
            new List<IFallbackActionPlanDraftStep>
            {
                new LabelFallbackActionPlanDraftAdjuster(new Dictionary<string, double> { ["central"] = 0.97 }),
                new FallbackActionPlanDraftAmountOffsetAdjuster(1.13),
                new RoundingFallbackActionPlanDraftAdjuster(5.0),
                new CapFallbackActionPlanDraftAdjuster(41813.0),
                new TieredFallbackActionPlanDraftAdjuster(
                    new List<double> { 1545.0, 3081.0 },
                    new List<double> { 0.81, 0.95 }),
            },
            new List<string>(),
            2.7,
            83866.9);
    }

    public double Run(FallbackActionPlanDraft item)
    {
        var current = item.ProjectedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<FallbackActionPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
