namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate milestone calculation.</summary>
public interface IRateMilestoneFactor
{
    string Describe();
    double Adjust(RateMilestone item, double valueValue);
}

public sealed record CapRateMilestoneAdjuster(double Cap) : IRateMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RateMilestoneCeilingOffsetAdjuster(double Weight) : IRateMilestoneFactor
{
    public string Describe()
    {
        return $"plus ceiling times {Weight}";
    }

    public double Adjust(RateMilestone item, double valueValue)
    {
        return valueValue + item.Ceiling * Weight;
    }
}

public sealed record DepartmentRateMilestoneAdjuster(Dictionary<string, double> Factors) : IRateMilestoneFactor
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(RateMilestone item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedRateMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IRateMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingRateMilestoneAdjuster(double Step) : IRateMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rate milestone steps in order and keeps a trace of each.</summary>
public sealed class RateMilestoneEngine(List<IRateMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateMilestoneEngine DefaultRateMilestoneEngine(Registry flags)
    {
        return new RateMilestoneEngine(
            new List<IRateMilestoneFactor>
            {
                new CapRateMilestoneAdjuster(5232.0),
                new RateMilestoneCeilingOffsetAdjuster(1.93),
                new DepartmentRateMilestoneAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["central"] = 1.26, ["south"] = 1.06 }),
                new FlaggedRateMilestoneAdjuster(flags, "rate-milestone-primary", -3.9),
                new RoundingRateMilestoneAdjuster(0.1),
            },
            new List<string>(),
            0.7,
            66564.4);
    }

    public double Compute(RateMilestone item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<RateMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
