namespace Acme.Workforce.Engagement.ActionPlan.Preference;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the manual action plan milestone calculation.</summary>
public interface IManualActionPlanMilestoneFactor
{
    string Describe();
    double Adjust(ManualActionPlanMilestone item, double valueValue);
}

public sealed record MinimumManualActionPlanMilestoneAdjuster(double Minimum) : IManualActionPlanMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(ManualActionPlanMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingManualActionPlanMilestoneAdjuster(double Step) : IManualActionPlanMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(ManualActionPlanMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapManualActionPlanMilestoneAdjuster(double Cap) : IManualActionPlanMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(ManualActionPlanMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record ManualActionPlanMilestoneExpiresAtOffsetAdjuster(double Weight) : IManualActionPlanMilestoneFactor
{
    public string Describe()
    {
        return $"plus expires at times {Weight}";
    }

    public double Adjust(ManualActionPlanMilestone item, double valueValue)
    {
        return valueValue + (double)item.ExpiresAt * Weight;
    }
}

public sealed record TieredManualActionPlanMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IManualActionPlanMilestoneFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(ManualActionPlanMilestone item, double valueValue)
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

/// <summary>Runs the manual action plan milestone steps in order and keeps a trace of each.</summary>
public sealed class ManualActionPlanMilestoneEngine(List<IManualActionPlanMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IManualActionPlanMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static ManualActionPlanMilestoneEngine DefaultManualActionPlanMilestoneEngine()
    {
        return new ManualActionPlanMilestoneEngine(
            new List<IManualActionPlanMilestoneFactor>
            {
                new MinimumManualActionPlanMilestoneAdjuster(21.4),
                new RoundingManualActionPlanMilestoneAdjuster(0.5),
                new CapManualActionPlanMilestoneAdjuster(38549.0),
                new ManualActionPlanMilestoneExpiresAtOffsetAdjuster(1.69),
                new TieredManualActionPlanMilestoneAdjuster(
                    new List<double> { 374.0, 2208.0, 3122.0, 9688.0 },
                    new List<double> { 1.19, 1.19, 1.01, 1.0 }),
            },
            new List<string>(),
            0.5,
            87263.5);
    }

    public double Evaluate(ManualActionPlanMilestone item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<ManualActionPlanMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
