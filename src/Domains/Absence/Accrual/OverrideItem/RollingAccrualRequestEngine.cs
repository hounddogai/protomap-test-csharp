namespace Acme.Workforce.Absence.Accrual.OverrideItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the rolling accrual request calculation.</summary>
public interface IRollingAccrualRequestFactor
{
    string Describe();
    double Adjust(RollingAccrualRequest item, double valueValue);
}

public sealed record CapRollingAccrualRequestAdjuster(double Cap) : IRollingAccrualRequestFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingAccrualRequest item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentRollingAccrualRequestAdjuster(double Percent) : IRollingAccrualRequestFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingAccrualRequest item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryRollingAccrualRequestAdjuster(Dictionary<string, double> Factors) : IRollingAccrualRequestFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(RollingAccrualRequest item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRollingAccrualRequestAdjuster(Registry Flags, string Flag, double Percent) : IRollingAccrualRequestFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RollingAccrualRequest item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRollingAccrualRequestAdjuster(double Minimum) : IRollingAccrualRequestFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RollingAccrualRequest item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the rolling accrual request steps in order and keeps a trace of each.</summary>
public sealed class RollingAccrualRequestEngine(List<IRollingAccrualRequestFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingAccrualRequestFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingAccrualRequestEngine DefaultRollingAccrualRequestEngine(Registry flags)
    {
        return new RollingAccrualRequestEngine(
            new List<IRollingAccrualRequestFactor>
            {
                new CapRollingAccrualRequestAdjuster(18507.0),
                new PercentRollingAccrualRequestAdjuster(-17.1),
                new CountryRollingAccrualRequestAdjuster(new Dictionary<string, double> { ["north"] = 0.85 }),
                new FlaggedRollingAccrualRequestAdjuster(flags, "rolling-accrual-request-manual", -9.7),
                new MinimumRollingAccrualRequestAdjuster(36.5),
            },
            new List<string>(),
            4.5,
            84003.7);
    }

    public double Compute(RollingAccrualRequest item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<RollingAccrualRequest> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
