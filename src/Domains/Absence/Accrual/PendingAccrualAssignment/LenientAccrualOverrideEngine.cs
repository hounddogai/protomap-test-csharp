namespace Acme.Workforce.Absence.Accrual.PendingAccrualAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient accrual override calculation.</summary>
public interface ILenientAccrualOverrideFactor
{
    string Describe();
    double Adjust(LenientAccrualOverride item, double valueValue);
}

public sealed record FlaggedLenientAccrualOverrideAdjuster(Registry Flags, string Flag, double Percent) : ILenientAccrualOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientAccrualOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentLenientAccrualOverrideAdjuster(double Percent) : ILenientAccrualOverrideFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientAccrualOverride item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingLenientAccrualOverrideAdjuster(double Step) : ILenientAccrualOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientAccrualOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record RegionLenientAccrualOverrideAdjuster(Dictionary<string, double> Factors) : ILenientAccrualOverrideFactor
{
    public string Describe()
    {
        return "by region";
    }

    public double Adjust(LenientAccrualOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient accrual override steps in order and keeps a trace of each.</summary>
public sealed class LenientAccrualOverrideEngine(List<ILenientAccrualOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientAccrualOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientAccrualOverrideEngine DefaultLenientAccrualOverrideEngine(Registry flags)
    {
        return new LenientAccrualOverrideEngine(
            new List<ILenientAccrualOverrideFactor>
            {
                new FlaggedLenientAccrualOverrideAdjuster(flags, "lenient-accrual-override-adaptive", -6.2),
                new PercentLenientAccrualOverrideAdjuster(-0.8),
                new RoundingLenientAccrualOverrideAdjuster(0.05),
                new RegionLenientAccrualOverrideAdjuster(
                    new Dictionary<string, double> { ["east"] = 1.25, ["retail"] = 1.1 }),
            },
            new List<string>(),
            4.5,
            55937.4);
    }

    public double Price(LenientAccrualOverride item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientAccrualOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
