namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount allocation calculation.</summary>
public interface IHeadcountAllocationStep
{
    string Describe();
    double Adjust(HeadcountAllocation item, double valueValue);
}

public sealed record PercentHeadcountAllocationAdjuster(double Percent) : IHeadcountAllocationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountAllocation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedHeadcountAllocationAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountAllocationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountAllocation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapHeadcountAllocationAdjuster(double Cap) : IHeadcountAllocationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountAllocation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record MinimumHeadcountAllocationAdjuster(double Minimum) : IHeadcountAllocationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountAllocation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingHeadcountAllocationAdjuster(double Step) : IHeadcountAllocationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountAllocation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the headcount allocation steps in order and keeps a trace of each.</summary>
public sealed class HeadcountAllocationEngine(List<IHeadcountAllocationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountAllocationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountAllocationEngine DefaultHeadcountAllocationEngine(Registry flags)
    {
        return new HeadcountAllocationEngine(
            new List<IHeadcountAllocationStep>
            {
                new PercentHeadcountAllocationAdjuster(19.6),
                new FlaggedHeadcountAllocationAdjuster(flags, "headcount-allocation-default", 5.0),
                new CapHeadcountAllocationAdjuster(17251.0),
                new MinimumHeadcountAllocationAdjuster(12.7),
                new RoundingHeadcountAllocationAdjuster(0.1),
            },
            new List<string>(),
            4.7,
            30503.3);
    }

    public double Price(HeadcountAllocation item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountAllocation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
