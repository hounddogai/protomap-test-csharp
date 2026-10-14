namespace Acme.Workforce.Compensation.MeritCycle.Allocation;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the merit cycle milestone calculation.</summary>
public interface IMeritCycleMilestoneModifier
{
    string Describe();
    double Adjust(MeritCycleMilestone item, double valueValue);
}

public sealed record PercentMeritCycleMilestoneAdjuster(double Percent) : IMeritCycleMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(MeritCycleMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingMeritCycleMilestoneAdjuster(double Step) : IMeritCycleMilestoneModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(MeritCycleMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumMeritCycleMilestoneAdjuster(double Minimum) : IMeritCycleMilestoneModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(MeritCycleMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LabelMeritCycleMilestoneAdjuster(Dictionary<string, double> Factors) : IMeritCycleMilestoneModifier
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(MeritCycleMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the merit cycle milestone steps in order and keeps a trace of each.</summary>
public sealed class MeritCycleMilestoneEngine(List<IMeritCycleMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IMeritCycleMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static MeritCycleMilestoneEngine DefaultMeritCycleMilestoneEngine()
    {
        return new MeritCycleMilestoneEngine(
            new List<IMeritCycleMilestoneModifier>
            {
                new PercentMeritCycleMilestoneAdjuster(-10.2),
                new RoundingMeritCycleMilestoneAdjuster(1.0),
                new MinimumMeritCycleMilestoneAdjuster(8.4),
                new LabelMeritCycleMilestoneAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.11, ["west"] = 0.89, ["retail"] = 0.74 }),
            },
            new List<string>(),
            0.5,
            41307.2);
    }

    public double Price(MeritCycleMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<MeritCycleMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
