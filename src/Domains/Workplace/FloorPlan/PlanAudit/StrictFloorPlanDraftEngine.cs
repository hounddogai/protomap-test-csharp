namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>One step of the strict floor plan draft calculation.</summary>
public interface IStrictFloorPlanDraftStep
{
    string Describe();
    double Adjust(StrictFloorPlanDraft item, double valueValue);
}

public sealed record TieredStrictFloorPlanDraftAdjuster(List<double> Bounds, List<double> Rates) : IStrictFloorPlanDraftStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(StrictFloorPlanDraft item, double valueValue)
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

public sealed record CapStrictFloorPlanDraftAdjuster(double Cap) : IStrictFloorPlanDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(StrictFloorPlanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamStrictFloorPlanDraftAdjuster(Dictionary<string, double> Factors) : IStrictFloorPlanDraftStep
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(StrictFloorPlanDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the strict floor plan draft steps in order and keeps a trace of each.</summary>
public sealed class StrictFloorPlanDraftEngine(List<IStrictFloorPlanDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IStrictFloorPlanDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static StrictFloorPlanDraftEngine DefaultStrictFloorPlanDraftEngine()
    {
        return new StrictFloorPlanDraftEngine(
            new List<IStrictFloorPlanDraftStep>
            {
                new TieredStrictFloorPlanDraftAdjuster(
                    new List<double> { 128.0, 441.0, 4333.0 },
                    new List<double> { 0.97, 1.19, 0.94 }),
                new CapStrictFloorPlanDraftAdjuster(7884.0),
                new TeamStrictFloorPlanDraftAdjuster(new Dictionary<string, double> { ["online"] = 1.2 }),
            },
            new List<string>(),
            0.6,
            47364.5);
    }

    public double Compute(StrictFloorPlanDraft item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<StrictFloorPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
