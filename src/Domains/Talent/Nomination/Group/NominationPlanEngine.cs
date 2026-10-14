namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the nomination plan calculation.</summary>
public interface INominationPlanModifier
{
    string Describe();
    double Adjust(NominationPlan item, double valueValue);
}

public sealed record FlaggedNominationPlanAdjuster(Registry Flags, string Flag, double Percent) : INominationPlanModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(NominationPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumNominationPlanAdjuster(double Minimum) : INominationPlanModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(NominationPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentNominationPlanAdjuster(double Percent) : INominationPlanModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(NominationPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapNominationPlanAdjuster(double Cap) : INominationPlanModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(NominationPlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TeamNominationPlanAdjuster(Dictionary<string, double> Factors) : INominationPlanModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(NominationPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the nomination plan steps in order and keeps a trace of each.</summary>
public sealed class NominationPlanEngine(List<INominationPlanModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<INominationPlanModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static NominationPlanEngine DefaultNominationPlanEngine(Registry flags)
    {
        return new NominationPlanEngine(
            new List<INominationPlanModifier>
            {
                new FlaggedNominationPlanAdjuster(flags, "nomination-plan-adaptive", -6.8),
                new MinimumNominationPlanAdjuster(46.1),
                new PercentNominationPlanAdjuster(-7.8),
                new CapNominationPlanAdjuster(32407.0),
                new TeamNominationPlanAdjuster(new Dictionary<string, double> { ["west"] = 0.9, ["central"] = 0.93 }),
            },
            new List<string>(),
            4.6,
            42572.8);
    }

    public double Evaluate(NominationPlan item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<NominationPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
