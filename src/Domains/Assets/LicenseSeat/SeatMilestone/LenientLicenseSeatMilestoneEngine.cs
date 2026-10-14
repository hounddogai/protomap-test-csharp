namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

/// <summary>One step of the lenient license seat milestone calculation.</summary>
public interface ILenientLicenseSeatMilestoneModifier
{
    string Describe();
    double Adjust(LenientLicenseSeatMilestone item, double valueValue);
}

public sealed record CapLenientLicenseSeatMilestoneAdjuster(double Cap) : ILenientLicenseSeatMilestoneModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientLicenseSeatMilestone item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentLenientLicenseSeatMilestoneAdjuster(double Percent) : ILenientLicenseSeatMilestoneModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LenientLicenseSeatMilestone item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LenientLicenseSeatMilestoneAttemptsOffsetAdjuster(double Weight) : ILenientLicenseSeatMilestoneModifier
{
    public string Describe()
    {
        return $"plus attempts times {Weight}";
    }

    public double Adjust(LenientLicenseSeatMilestone item, double valueValue)
    {
        return valueValue + (double)item.Attempts * Weight;
    }
}

/// <summary>Runs the lenient license seat milestone steps in order and keeps a trace of each.</summary>
public sealed class LenientLicenseSeatMilestoneEngine(List<ILenientLicenseSeatMilestoneModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientLicenseSeatMilestoneModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientLicenseSeatMilestoneEngine DefaultLenientLicenseSeatMilestoneEngine()
    {
        return new LenientLicenseSeatMilestoneEngine(
            new List<ILenientLicenseSeatMilestoneModifier>
            {
                new CapLenientLicenseSeatMilestoneAdjuster(47816.0),
                new PercentLenientLicenseSeatMilestoneAdjuster(22.8),
                new LenientLicenseSeatMilestoneAttemptsOffsetAdjuster(0.76),
            },
            new List<string>(),
            4.6,
            78097.2);
    }

    public double Price(LenientLicenseSeatMilestone item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientLicenseSeatMilestone> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
