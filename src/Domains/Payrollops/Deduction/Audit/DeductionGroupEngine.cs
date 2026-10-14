namespace Acme.Workforce.Payrollops.Deduction.Audit;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the deduction group calculation.</summary>
public interface IDeductionGroupModifier
{
    string Describe();
    double Adjust(DeductionGroup item, double valueValue);
}

public sealed record FlaggedDeductionGroupAdjuster(Registry Flags, string Flag, double Percent) : IDeductionGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeductionGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DeductionGroupDiscountOffsetAdjuster(double Weight) : IDeductionGroupModifier
{
    public string Describe()
    {
        return $"plus discount times {Weight}";
    }

    public double Adjust(DeductionGroup item, double valueValue)
    {
        return valueValue + item.Discount * Weight;
    }
}

public sealed record CapDeductionGroupAdjuster(double Cap) : IDeductionGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeductionGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingDeductionGroupAdjuster(double Step) : IDeductionGroupModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeductionGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record ChannelDeductionGroupAdjuster(Dictionary<string, double> Factors) : IDeductionGroupModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(DeductionGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentDeductionGroupAdjuster(double Percent) : IDeductionGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeductionGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the deduction group steps in order and keeps a trace of each.</summary>
public sealed class DeductionGroupEngine(List<IDeductionGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeductionGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeductionGroupEngine DefaultDeductionGroupEngine(Registry flags)
    {
        return new DeductionGroupEngine(
            new List<IDeductionGroupModifier>
            {
                new FlaggedDeductionGroupAdjuster(flags, "deduction-group-adaptive", -9.9),
                new DeductionGroupDiscountOffsetAdjuster(0.51),
                new CapDeductionGroupAdjuster(29159.0),
                new RoundingDeductionGroupAdjuster(1.0),
                new ChannelDeductionGroupAdjuster(new Dictionary<string, double> { ["north"] = 1.37 }),
                new PercentDeductionGroupAdjuster(17.8),
            },
            new List<string>(),
            2.5,
            17569.5);
    }

    public double Price(DeductionGroup item)
    {
        var current = item.AdjustedTotal();
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

    public Dictionary<string, double> EvaluateAll(List<DeductionGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
