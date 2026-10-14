namespace Acme.Workforce.Engagement.ActionPlan.Profile;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending action plan baseline calculation.</summary>
public interface IPendingActionPlanBaselineModifier
{
    string Describe();
    double Adjust(PendingActionPlanBaseline item, double valueValue);
}

public sealed record CapPendingActionPlanBaselineAdjuster(double Cap) : IPendingActionPlanBaselineModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingActionPlanBaseline item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record NamePendingActionPlanBaselineAdjuster(Dictionary<string, double> Factors) : IPendingActionPlanBaselineModifier
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(PendingActionPlanBaseline item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPendingActionPlanBaselineAdjuster(Registry Flags, string Flag, double Percent) : IPendingActionPlanBaselineModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingActionPlanBaseline item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingPendingActionPlanBaselineAdjuster(double Step) : IPendingActionPlanBaselineModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PendingActionPlanBaseline item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PendingActionPlanBaselineRevisionOffsetAdjuster(double Weight) : IPendingActionPlanBaselineModifier
{
    public string Describe()
    {
        return $"plus revision times {Weight}";
    }

    public double Adjust(PendingActionPlanBaseline item, double valueValue)
    {
        return valueValue + (double)item.Revision * Weight;
    }
}

/// <summary>Runs the pending action plan baseline steps in order and keeps a trace of each.</summary>
public sealed class PendingActionPlanBaselineEngine(List<IPendingActionPlanBaselineModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingActionPlanBaselineModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingActionPlanBaselineEngine DefaultPendingActionPlanBaselineEngine(Registry flags)
    {
        return new PendingActionPlanBaselineEngine(
            new List<IPendingActionPlanBaselineModifier>
            {
                new CapPendingActionPlanBaselineAdjuster(17091.0),
                new NamePendingActionPlanBaselineAdjuster(new Dictionary<string, double> { ["online"] = 1.26 }),
                new FlaggedPendingActionPlanBaselineAdjuster(flags, "pending-action-plan-baseline-lenient", 1.6),
                new RoundingPendingActionPlanBaselineAdjuster(0.5),
                new PendingActionPlanBaselineRevisionOffsetAdjuster(0.98),
            },
            new List<string>(),
            3.0,
            20535.4);
    }

    public double Evaluate(PendingActionPlanBaseline item)
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

    public Dictionary<string, double> EvaluateAll(List<PendingActionPlanBaseline> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
