namespace Acme.Workforce.Talent.SuccessionPlan.Variance;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the succession plan draft calculation.</summary>
public interface ISuccessionPlanDraftStep
{
    string Describe();
    double Adjust(SuccessionPlanDraft item, double valueValue);
}

public sealed record FlaggedSuccessionPlanDraftAdjuster(Registry Flags, string Flag, double Percent) : ISuccessionPlanDraftStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(SuccessionPlanDraft item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingSuccessionPlanDraftAdjuster(double Step) : ISuccessionPlanDraftStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(SuccessionPlanDraft item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record SuccessionPlanDraftCeilingOffsetAdjuster(double Weight) : ISuccessionPlanDraftStep
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(SuccessionPlanDraft item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record MinimumSuccessionPlanDraftAdjuster(double Minimum) : ISuccessionPlanDraftStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(SuccessionPlanDraft item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RegionSuccessionPlanDraftAdjuster(Dictionary<string, double> Factors) : ISuccessionPlanDraftStep
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(SuccessionPlanDraft item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the succession plan draft steps in order and keeps a trace of each.</summary>
public sealed class SuccessionPlanDraftEngine(List<ISuccessionPlanDraftStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ISuccessionPlanDraftStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static SuccessionPlanDraftEngine DefaultSuccessionPlanDraftEngine(Registry flags)
    {
        return new SuccessionPlanDraftEngine(
            new List<ISuccessionPlanDraftStep>
            {
                new FlaggedSuccessionPlanDraftAdjuster(flags, "succession-plan-draft-manual", -9.5),
                new RoundingSuccessionPlanDraftAdjuster(5.0),
                new SuccessionPlanDraftCeilingOffsetAdjuster(0.49),
                new MinimumSuccessionPlanDraftAdjuster(11.3),
                new RegionSuccessionPlanDraftAdjuster(new Dictionary<string, double> { ["central"] = 0.94 }),
            },
            new List<string>(),
            4.3,
            3369.3);
    }

    public double Price(SuccessionPlanDraft item)
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

    public Dictionary<string, double> EvaluateAll(List<SuccessionPlanDraft> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
