namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the return plan claim calculation.</summary>
public interface IReturnPlanClaimAdjuster
{
    string Describe();
    double Adjust(ReturnPlanClaim item, double valueValue);
}

public sealed record SegmentReturnPlanClaimAdjuster(Dictionary<string, double> Factors) : IReturnPlanClaimAdjuster
{
    public string Describe()
    {
        return "by segment";
    }

    public double Adjust(ReturnPlanClaim item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingReturnPlanClaimAdjuster(double Step) : IReturnPlanClaimAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ReturnPlanClaim item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapReturnPlanClaimAdjuster(double Cap) : IReturnPlanClaimAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ReturnPlanClaim item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the return plan claim steps in order and keeps a trace of each.</summary>
public sealed class ReturnPlanClaimEngine(List<IReturnPlanClaimAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IReturnPlanClaimAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ReturnPlanClaimEngine DefaultReturnPlanClaimEngine()
    {
        return new ReturnPlanClaimEngine(
            new List<IReturnPlanClaimAdjuster>
            {
                new SegmentReturnPlanClaimAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.78, ["north"] = 1.19 }),
                new RoundingReturnPlanClaimAdjuster(0.5),
                new CapReturnPlanClaimAdjuster(10347.0),
            },
            new List<string>(),
            5.0,
            93186.0);
    }

    public double Compute(ReturnPlanClaim item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<ReturnPlanClaim> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
