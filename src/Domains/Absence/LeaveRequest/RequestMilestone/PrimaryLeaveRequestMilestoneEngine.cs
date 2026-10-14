namespace Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the primary leave request milestone calculation.</summary>
public interface IPrimaryLeaveRequestMilestoneFactor
{
    string Describe();
    double Adjust(PrimaryLeaveRequestMilestone item, double valueValue);
}

public sealed record TieredPrimaryLeaveRequestMilestoneAdjuster(List<double> Bounds, List<double> Rates) : IPrimaryLeaveRequestMilestoneFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PrimaryLeaveRequestMilestone item, double valueValue)
    {
        for (var index = 0; index < Bounds.Count; index++)
        {
            if (valueValue <= Bounds[index] && index < Rates.Count)
            {
                return valueValue * Rates[index];
            }
        }
        return valueValue;
    }
}

public sealed record PercentPrimaryLeaveRequestMilestoneAdjuster(double Percent) : IPrimaryLeaveRequestMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PrimaryLeaveRequestMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPrimaryLeaveRequestMilestoneAdjuster(double Minimum) : IPrimaryLeaveRequestMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PrimaryLeaveRequestMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingPrimaryLeaveRequestMilestoneAdjuster(double Step) : IPrimaryLeaveRequestMilestoneFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(PrimaryLeaveRequestMilestone item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapPrimaryLeaveRequestMilestoneAdjuster(double Cap) : IPrimaryLeaveRequestMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PrimaryLeaveRequestMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the primary leave request milestone steps in order and keeps a trace of each.</summary>
public sealed class PrimaryLeaveRequestMilestoneEngine(List<IPrimaryLeaveRequestMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPrimaryLeaveRequestMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PrimaryLeaveRequestMilestoneEngine DefaultPrimaryLeaveRequestMilestoneEngine()
    {
        return new PrimaryLeaveRequestMilestoneEngine(
            new List<IPrimaryLeaveRequestMilestoneFactor>
            {
                new TieredPrimaryLeaveRequestMilestoneAdjuster(
                    new List<double> { 6050.0, 7834.0 },
                    new List<double> { 1.16, 1.14 }),
                new PercentPrimaryLeaveRequestMilestoneAdjuster(15.0),
                new MinimumPrimaryLeaveRequestMilestoneAdjuster(46.0),
                new RoundingPrimaryLeaveRequestMilestoneAdjuster(0.25),
                new CapPrimaryLeaveRequestMilestoneAdjuster(46281.0),
            },
            new List<string>(),
            1.8,
            57793.6);
    }

    public double Price(PrimaryLeaveRequestMilestone item)
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

    public Dictionary<string, double> EvaluateAll(List<PrimaryLeaveRequestMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
