namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional bonus pool override calculation.</summary>
public interface IRegionalBonusPoolOverrideFactor
{
    string Describe();
    double Adjust(RegionalBonusPoolOverride item, double valueValue);
}

public sealed record CategoryRegionalBonusPoolOverrideAdjuster(Dictionary<string, double> Factors) : IRegionalBonusPoolOverrideFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RegionalBonusPoolOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapRegionalBonusPoolOverrideAdjuster(double Cap) : IRegionalBonusPoolOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalBonusPoolOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRegionalBonusPoolOverrideAdjuster(double Percent) : IRegionalBonusPoolOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalBonusPoolOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRegionalBonusPoolOverrideAdjuster(double Step) : IRegionalBonusPoolOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalBonusPoolOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumRegionalBonusPoolOverrideAdjuster(double Minimum) : IRegionalBonusPoolOverrideFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalBonusPoolOverride item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedRegionalBonusPoolOverrideAdjuster(Registry Flags, string Flag, double Percent) : IRegionalBonusPoolOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalBonusPoolOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the regional bonus pool override steps in order and keeps a trace of each.</summary>
public sealed class RegionalBonusPoolOverrideEngine(List<IRegionalBonusPoolOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalBonusPoolOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalBonusPoolOverrideEngine DefaultRegionalBonusPoolOverrideEngine(Registry flags)
    {
        return new RegionalBonusPoolOverrideEngine(
            new List<IRegionalBonusPoolOverrideFactor>
            {
                new CategoryRegionalBonusPoolOverrideAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.14, ["central"] = 0.75, ["north"] = 1.21 }),
                new CapRegionalBonusPoolOverrideAdjuster(48983.0),
                new PercentRegionalBonusPoolOverrideAdjuster(24.9),
                new RoundingRegionalBonusPoolOverrideAdjuster(5.0),
                new MinimumRegionalBonusPoolOverrideAdjuster(10.4),
                new FlaggedRegionalBonusPoolOverrideAdjuster(flags, "regional-bonus-pool-override-strict", -5.1),
            },
            new List<string>(),
            1.8,
            45748.4);
    }

    public double Evaluate(RegionalBonusPoolOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<RegionalBonusPoolOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
