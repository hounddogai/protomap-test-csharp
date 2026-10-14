namespace Acme.Workforce.Engagement.ActionPlan.Claim;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient action plan exception calculation.</summary>
public interface ILenientActionPlanExceptionAdjuster
{
    string Describe();
    double Adjust(LenientActionPlanException item, double valueValue);
}

public sealed record LabelLenientActionPlanExceptionAdjuster(Dictionary<string, double> Factors) : ILenientActionPlanExceptionAdjuster
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LenientActionPlanException item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLenientActionPlanExceptionAdjuster(Registry Flags, string Flag, double Percent) : ILenientActionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientActionPlanException item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientActionPlanExceptionGraceDaysOffsetAdjuster(double Weight) : ILenientActionPlanExceptionAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(LenientActionPlanException item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

/// <summary>Runs the lenient action plan exception steps in order and keeps a trace of each.</summary>
public sealed class LenientActionPlanExceptionEngine(List<ILenientActionPlanExceptionAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientActionPlanExceptionAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientActionPlanExceptionEngine DefaultLenientActionPlanExceptionEngine(Registry flags)
    {
        return new LenientActionPlanExceptionEngine(
            new List<ILenientActionPlanExceptionAdjuster>
            {
                new LabelLenientActionPlanExceptionAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.36, ["west"] = 0.89 }),
                new FlaggedLenientActionPlanExceptionAdjuster(flags, "lenient-action-plan-exception-fallback", 1.3),
                new LenientActionPlanExceptionGraceDaysOffsetAdjuster(1.55),
            },
            new List<string>(),
            4.5,
            94086.2);
    }

    public double Compute(LenientActionPlanException item)
    {
        var current = item.AdjustedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LenientActionPlanException> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
