namespace Acme.Workforce.Assets.Refresh.Rollup;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh rollup calculation.</summary>
public interface IRefreshRollupFactor
{
    string Describe();
    double Adjust(RefreshRollup item, double valueValue);
}

public sealed record TeamRefreshRollupAdjuster(Dictionary<string, double> Factors) : IRefreshRollupFactor
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(RefreshRollup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRefreshRollupAdjuster(Registry Flags, string Flag, double Percent) : IRefreshRollupFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshRollup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRefreshRollupAdjuster(double Step) : IRefreshRollupFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshRollup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRefreshRollupAdjuster(double Cap) : IRefreshRollupFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshRollup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the refresh rollup steps in order and keeps a trace of each.</summary>
public sealed class RefreshRollupEngine(List<IRefreshRollupFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshRollupFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshRollupEngine DefaultRefreshRollupEngine(Registry flags)
    {
        return new RefreshRollupEngine(
            new List<IRefreshRollupFactor>
            {
                new TeamRefreshRollupAdjuster(new Dictionary<string, double> { ["west"] = 0.87, ["retail"] = 1.08 }),
                new FlaggedRefreshRollupAdjuster(flags, "refresh-rollup-lenient", -5.2),
                new RoundingRefreshRollupAdjuster(0.1),
                new CapRefreshRollupAdjuster(18517.0),
            },
            new List<string>(),
            4.4,
            87877.7);
    }

    public double Evaluate(RefreshRollup item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshRollup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
