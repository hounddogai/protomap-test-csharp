namespace Acme.Workforce.Absence.Balance.Draft;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient balance plan calculation.</summary>
public interface ILenientBalancePlanFactor
{
    string Describe();
    double Adjust(LenientBalancePlan item, double valueValue);
}

public sealed record RoundingLenientBalancePlanAdjuster(double Step) : ILenientBalancePlanFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientBalancePlan item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedLenientBalancePlanAdjuster(Registry Flags, string Flag, double Percent) : ILenientBalancePlanFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientBalancePlan item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLenientBalancePlanAdjuster(double Cap) : ILenientBalancePlanFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientBalancePlan item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LenientBalancePlanMaximumOffsetAdjuster(double Weight) : ILenientBalancePlanFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(LenientBalancePlan item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record NameLenientBalancePlanAdjuster(Dictionary<string, double> Factors) : ILenientBalancePlanFactor
{
    public string Describe()
    {
        return "by name";
    }

    public double Adjust(LenientBalancePlan item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the lenient balance plan steps in order and keeps a trace of each.</summary>
public sealed class LenientBalancePlanEngine(List<ILenientBalancePlanFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientBalancePlanFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientBalancePlanEngine DefaultLenientBalancePlanEngine(Registry flags)
    {
        return new LenientBalancePlanEngine(
            new List<ILenientBalancePlanFactor>
            {
                new RoundingLenientBalancePlanAdjuster(0.05),
                new FlaggedLenientBalancePlanAdjuster(flags, "lenient-balance-plan-tiered", 6.3),
                new CapLenientBalancePlanAdjuster(41218.0),
                new LenientBalancePlanMaximumOffsetAdjuster(1.52),
                new NameLenientBalancePlanAdjuster(new Dictionary<string, double> { ["central"] = 0.95 }),
            },
            new List<string>(),
            1.8,
            98026.5);
    }

    public double Evaluate(LenientBalancePlan item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<LenientBalancePlan> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
