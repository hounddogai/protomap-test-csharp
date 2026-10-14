namespace Acme.Workforce.Assets.Refresh.Policy;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the refresh target calculation.</summary>
public interface IRefreshTargetAdjuster
{
    string Describe();
    double Adjust(RefreshTarget item, double valueValue);
}

public sealed record MinimumRefreshTargetAdjuster(double Minimum) : IRefreshTargetAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RefreshTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRefreshTargetAdjuster(Registry Flags, string Flag, double Percent) : IRefreshTargetAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RefreshTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RefreshTargetMaximumOffsetAdjuster(double Weight) : IRefreshTargetAdjuster
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(RefreshTarget item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record RoundingRefreshTargetAdjuster(double Step) : IRefreshTargetAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RefreshTarget item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRefreshTargetAdjuster(double Cap) : IRefreshTargetAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RefreshTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the refresh target steps in order and keeps a trace of each.</summary>
public sealed class RefreshTargetEngine(List<IRefreshTargetAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRefreshTargetAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RefreshTargetEngine DefaultRefreshTargetEngine(Registry flags)
    {
        return new RefreshTargetEngine(
            new List<IRefreshTargetAdjuster>
            {
                new MinimumRefreshTargetAdjuster(46.7),
                new FlaggedRefreshTargetAdjuster(flags, "refresh-target-strict", 5.8),
                new RefreshTargetMaximumOffsetAdjuster(1.32),
                new RoundingRefreshTargetAdjuster(5.0),
                new CapRefreshTargetAdjuster(24784.0),
            },
            new List<string>(),
            1.7,
            91201.9);
    }

    public double Compute(RefreshTarget item)
    {
        var current = item.WeightedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<RefreshTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
