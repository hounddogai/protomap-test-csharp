namespace Acme.Workforce.Assets.Refresh.Estimate;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling refresh window calculation.</summary>
public interface IRollingRefreshWindowModifier
{
    string Describe();
    double Adjust(RollingRefreshWindow item, double valueValue);
}

public sealed record FlaggedRollingRefreshWindowAdjuster(Registry Flags, string Flag, double Percent) : IRollingRefreshWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingRefreshWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RollingRefreshWindowSequenceOffsetAdjuster(double Weight) : IRollingRefreshWindowModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(RollingRefreshWindow item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record CapRollingRefreshWindowAdjuster(double Cap) : IRollingRefreshWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingRefreshWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rolling refresh window steps in order and keeps a trace of each.</summary>
public sealed class RollingRefreshWindowEngine(List<IRollingRefreshWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingRefreshWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingRefreshWindowEngine DefaultRollingRefreshWindowEngine(Registry flags)
    {
        return new RollingRefreshWindowEngine(
            new List<IRollingRefreshWindowModifier>
            {
                new FlaggedRollingRefreshWindowAdjuster(flags, "rolling-refresh-window-pending", -9.9),
                new RollingRefreshWindowSequenceOffsetAdjuster(1.63),
                new CapRollingRefreshWindowAdjuster(38402.0),
            },
            new List<string>(),
            2.2,
            64217.2);
    }

    public double Price(RollingRefreshWindow item)
    {
        var current = item.EffectiveTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RollingRefreshWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
