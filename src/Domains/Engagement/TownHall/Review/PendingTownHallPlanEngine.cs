namespace Acme.Workforce.Engagement.TownHall.Review;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending town hall plan calculation.</summary>
public interface IPendingTownHallPlanAdjuster
{
    string Describe();
    double Adjust(PendingTownHallPlan item, double valueValue);
}

public sealed record TieredPendingTownHallPlanAdjuster(List<double> Bounds, List<double> Rates) : IPendingTownHallPlanAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingTownHallPlan item, double valueValue)
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

public sealed record TitlePendingTownHallPlanAdjuster(Dictionary<string, double> Factors) : IPendingTownHallPlanAdjuster
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PendingTownHallPlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedPendingTownHallPlanAdjuster(Registry Flags, string Flag, double Percent) : IPendingTownHallPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingTownHallPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingTownHallPlanGraceDaysOffsetAdjuster(double Weight) : IPendingTownHallPlanAdjuster
{
    public string Describe()
    {
        return $"plus grace days times {Weight}";
    }

    public double Adjust(PendingTownHallPlan item, double valueValue)
    {
        return valueValue + (double)item.GraceDays * Weight;
    }
}

public sealed record PercentPendingTownHallPlanAdjuster(double Percent) : IPendingTownHallPlanAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingTownHallPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pending town hall plan steps in order and keeps a trace of each.</summary>
public sealed class PendingTownHallPlanEngine(List<IPendingTownHallPlanAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingTownHallPlanAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingTownHallPlanEngine DefaultPendingTownHallPlanEngine(Registry flags)
    {
        return new PendingTownHallPlanEngine(
            new List<IPendingTownHallPlanAdjuster>
            {
                new TieredPendingTownHallPlanAdjuster(
                    new List<double> { 1560.0, 7198.0 },
                    new List<double> { 1.2, 0.91 }),
                new TitlePendingTownHallPlanAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.21, ["retail"] = 0.79, ["south"] = 0.92 }),
                new FlaggedPendingTownHallPlanAdjuster(flags, "pending-town-hall-plan-adaptive", -3.7),
                new PendingTownHallPlanGraceDaysOffsetAdjuster(1.27),
                new PercentPendingTownHallPlanAdjuster(9.1),
            },
            new List<string>(),
            4.8,
            20080.5);
    }

    public double Compute(PendingTownHallPlan item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PendingTownHallPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
