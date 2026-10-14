namespace Acme.Workforce.Assets.Refresh.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh plan calculation.</summary>
public interface IRefreshPlanFactor
{
    string Describe();
    double Adjust(RefreshPlan item, double valueValue);
}

public sealed record PercentRefreshPlanAdjuster(double Percent) : IRefreshPlanFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RefreshPlan item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRefreshPlanAdjuster(List<double> Bounds, List<double> Rates) : IRefreshPlanFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RefreshPlan item, double valueValue)
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

public sealed record RefreshPlanFteOffsetAdjuster(double Weight) : IRefreshPlanFactor
{
    public string Describe()
    {
        return $"plus fte times {Weight}";
    }

    public double Adjust(RefreshPlan item, double valueValue)
    {
        return valueValue + item.Fte * Weight;
    }
}

public sealed record FlaggedRefreshPlanAdjuster(Registry Flags, string Flag, double Percent) : IRefreshPlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshPlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRefreshPlanAdjuster(double Minimum) : IRefreshPlanFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshPlan item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the refresh plan steps in order and keeps a trace of each.</summary>
public sealed class RefreshPlanEngine(List<IRefreshPlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshPlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshPlanEngine DefaultRefreshPlanEngine(Registry flags)
    {
        return new RefreshPlanEngine(
            new List<IRefreshPlanFactor>
            {
                new PercentRefreshPlanAdjuster(-2.9),
                new TieredRefreshPlanAdjuster(new List<double> { 2695.0, 4299.0 }, new List<double> { 1.02, 1.19 }),
                new RefreshPlanFteOffsetAdjuster(0.3),
                new FlaggedRefreshPlanAdjuster(flags, "refresh-plan-default", 6.6),
                new MinimumRefreshPlanAdjuster(12.4),
            },
            new List<string>(),
            1.1,
            77588.6);
    }

    public double Compute(RefreshPlan item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshPlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
