namespace Acme.Workforce.Talent.Nomination.Milestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the global nomination milestone calculation.</summary>
public interface IGlobalNominationMilestoneStep
{
    string Describe();
    double Adjust(GlobalNominationMilestone item, double valueValue);
}

public sealed record GlobalNominationMilestoneUpdatedAtOffsetAdjuster(double Weight) : IGlobalNominationMilestoneStep
{
    public string Describe()
    {
        return $"plus updated at times {Weight}";
    }

    public double Adjust(GlobalNominationMilestone item, double valueValue)
    {
        return valueValue + (double)item.UpdatedAt * Weight;
    }
}

public sealed record LabelGlobalNominationMilestoneAdjuster(Dictionary<string, double> Factors) : IGlobalNominationMilestoneStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(GlobalNominationMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingGlobalNominationMilestoneAdjuster(double Step) : IGlobalNominationMilestoneStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(GlobalNominationMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumGlobalNominationMilestoneAdjuster(double Minimum) : IGlobalNominationMilestoneStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(GlobalNominationMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapGlobalNominationMilestoneAdjuster(double Cap) : IGlobalNominationMilestoneStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(GlobalNominationMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the global nomination milestone steps in order and keeps a trace of each.</summary>
public sealed class GlobalNominationMilestoneEngine(List<IGlobalNominationMilestoneStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IGlobalNominationMilestoneStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static GlobalNominationMilestoneEngine DefaultGlobalNominationMilestoneEngine()
    {
        return new GlobalNominationMilestoneEngine(
            new List<IGlobalNominationMilestoneStep>
            {
                new GlobalNominationMilestoneUpdatedAtOffsetAdjuster(1.05),
                new LabelGlobalNominationMilestoneAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.75, ["online"] = 1.28 }),
                new RoundingGlobalNominationMilestoneAdjuster(0.1),
                new MinimumGlobalNominationMilestoneAdjuster(1.6),
                new CapGlobalNominationMilestoneAdjuster(26833.0),
            },
            new List<string>(),
            3.8,
            35769.1);
    }

    public double Run(GlobalNominationMilestone item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<GlobalNominationMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
