namespace Acme.Workforce.Absence.Accrual.Threshold;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the accrual window calculation.</summary>
public interface IAccrualWindowModifier
{
    string Describe();
    double Adjust(AccrualWindow item, double valueValue);
}

public sealed record CapAccrualWindowAdjuster(double Cap) : IAccrualWindowModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AccrualWindow item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingAccrualWindowAdjuster(double Step) : IAccrualWindowModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccrualWindow item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LocaleAccrualWindowAdjuster(Dictionary<string, double> Factors) : IAccrualWindowModifier
{
    public string Describe()
    {
        return "by locale";
    }

    public double Adjust(AccrualWindow item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAccrualWindowAdjuster(Registry Flags, string Flag, double Percent) : IAccrualWindowModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AccrualWindow item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumAccrualWindowAdjuster(double Minimum) : IAccrualWindowModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccrualWindow item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

/// <summary>Runs the accrual window steps in order and keeps a trace of each.</summary>
public sealed class AccrualWindowEngine(List<IAccrualWindowModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccrualWindowModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccrualWindowEngine DefaultAccrualWindowEngine(Registry flags)
    {
        return new AccrualWindowEngine(
            new List<IAccrualWindowModifier>
            {
                new CapAccrualWindowAdjuster(41959.0),
                new RoundingAccrualWindowAdjuster(0.25),
                new LocaleAccrualWindowAdjuster(new Dictionary<string, double> { ["retail"] = 1.32 }),
                new FlaggedAccrualWindowAdjuster(flags, "accrual-window-tiered", -5.9),
                new MinimumAccrualWindowAdjuster(41.4),
            },
            new List<string>(),
            2.9,
            45422.6);
    }

    public double Run(AccrualWindow item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<AccrualWindow> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
