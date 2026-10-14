namespace Acme.Workforce.Engagement.ActionPlan.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the action plan draft calculation.</summary>
public interface IActionPlanDraftStep
{
    string Describe();
    double Adjust(ActionPlanDraft item, double valueValue);
}

public sealed record CapActionPlanDraftAdjuster(double Cap) : IActionPlanDraftStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ActionPlanDraft item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumActionPlanDraftAdjuster(double Minimum) : IActionPlanDraftStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ActionPlanDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record ActionPlanDraftLimitOffsetAdjuster(double Weight) : IActionPlanDraftStep
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(ActionPlanDraft item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentActionPlanDraftAdjuster(double Percent) : IActionPlanDraftStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(ActionPlanDraft item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingActionPlanDraftAdjuster(double Step) : IActionPlanDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ActionPlanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the action plan draft steps in order and keeps a trace of each.</summary>
public sealed class ActionPlanDraftEngine(List<IActionPlanDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IActionPlanDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ActionPlanDraftEngine DefaultActionPlanDraftEngine()
    {
        return new ActionPlanDraftEngine(
            new List<IActionPlanDraftStep>
            {
                new CapActionPlanDraftAdjuster(26026.0),
                new MinimumActionPlanDraftAdjuster(0.5),
                new ActionPlanDraftLimitOffsetAdjuster(0.88),
                new PercentActionPlanDraftAdjuster(24.2),
                new RoundingActionPlanDraftAdjuster(0.25),
            },
            new List<string>(),
            1.6,
            51205.3);
    }

    public double Compute(ActionPlanDraft item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ActionPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
