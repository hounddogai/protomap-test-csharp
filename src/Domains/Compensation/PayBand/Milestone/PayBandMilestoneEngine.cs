namespace Acme.Workforce.Compensation.PayBand.Milestone;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pay band milestone calculation.</summary>
public interface IPayBandMilestoneFactor
{
    string Describe();
    double Adjust(PayBandMilestone item, double valueValue);
}

public sealed record FlaggedPayBandMilestoneAdjuster(Registry Flags, string Flag, double Percent) : IPayBandMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PayBandMilestone item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumPayBandMilestoneAdjuster(double Minimum) : IPayBandMilestoneFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PayBandMilestone item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPayBandMilestoneAdjuster(double Cap) : IPayBandMilestoneFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PayBandMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPayBandMilestoneAdjuster(double Percent) : IPayBandMilestoneFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PayBandMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the pay band milestone steps in order and keeps a trace of each.</summary>
public sealed class PayBandMilestoneEngine(List<IPayBandMilestoneFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPayBandMilestoneFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PayBandMilestoneEngine DefaultPayBandMilestoneEngine(Registry flags)
    {
        return new PayBandMilestoneEngine(
            new List<IPayBandMilestoneFactor>
            {
                new FlaggedPayBandMilestoneAdjuster(flags, "pay-band-milestone-seasonal", -1.3),
                new MinimumPayBandMilestoneAdjuster(48.9),
                new CapPayBandMilestoneAdjuster(39154.0),
                new PercentPayBandMilestoneAdjuster(11.2),
            },
            new List<string>(),
            0.4,
            45445.5);
    }

    public double Price(PayBandMilestone item)
    {
        var current = item.EffectiveVolume();
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

    public Dictionary<string, double> EvaluateAll(List<PayBandMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
