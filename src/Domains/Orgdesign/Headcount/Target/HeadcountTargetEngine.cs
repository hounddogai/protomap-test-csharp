namespace Acme.Workforce.Orgdesign.Headcount.Target;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the headcount target calculation.</summary>
public interface IHeadcountTargetModifier
{
    string Describe();
    double Adjust(HeadcountTarget item, double valueValue);
}

public sealed record CapHeadcountTargetAdjuster(double Cap) : IHeadcountTargetModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountTarget item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record HeadcountTargetRetriesOffsetAdjuster(double Weight) : IHeadcountTargetModifier
{
    public string Describe()
    {
        return $"plus retries times {Weight}";
    }

    public double Adjust(HeadcountTarget item, double valueValue)
    {
        return valueValue + (double)item.Retries * Weight;
    }
}

public sealed record MinimumHeadcountTargetAdjuster(double Minimum) : IHeadcountTargetModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountTarget item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record DepartmentHeadcountTargetAdjuster(Dictionary<string, double> Factors) : IHeadcountTargetModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(HeadcountTarget item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedHeadcountTargetAdjuster(Registry Flags, string Flag, double Percent) : IHeadcountTargetModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(HeadcountTarget item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the headcount target steps in order and keeps a trace of each.</summary>
public sealed class HeadcountTargetEngine(List<IHeadcountTargetModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountTargetModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountTargetEngine DefaultHeadcountTargetEngine(Registry flags)
    {
        return new HeadcountTargetEngine(
            new List<IHeadcountTargetModifier>
            {
                new CapHeadcountTargetAdjuster(20075.0),
                new HeadcountTargetRetriesOffsetAdjuster(1.54),
                new MinimumHeadcountTargetAdjuster(36.8),
                new DepartmentHeadcountTargetAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.92, ["central"] = 1.39, ["online"] = 1.35 }),
                new FlaggedHeadcountTargetAdjuster(flags, "headcount-target-weighted", -9.9),
            },
            new List<string>(),
            2.9,
            87397.7);
    }

    public double Compute(HeadcountTarget item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<HeadcountTarget> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
