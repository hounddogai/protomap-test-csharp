namespace Acme.Workforce.Contractors.Engagement.Reservation;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the weighted engagement reservation calculation.</summary>
public interface IWeightedEngagementReservationStep
{
    string Describe();
    double Adjust(WeightedEngagementReservation item, double valueValue);
}

public sealed record FlaggedWeightedEngagementReservationAdjuster(Registry Flags, string Flag, double Percent) : IWeightedEngagementReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WeightedEngagementReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CountryWeightedEngagementReservationAdjuster(Dictionary<string, double> Factors) : IWeightedEngagementReservationStep
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(WeightedEngagementReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingWeightedEngagementReservationAdjuster(double Step) : IWeightedEngagementReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WeightedEngagementReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumWeightedEngagementReservationAdjuster(double Minimum) : IWeightedEngagementReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(WeightedEngagementReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapWeightedEngagementReservationAdjuster(double Cap) : IWeightedEngagementReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(WeightedEngagementReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentWeightedEngagementReservationAdjuster(double Percent) : IWeightedEngagementReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WeightedEngagementReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the weighted engagement reservation steps in order and keeps a trace of each.</summary>
public sealed class WeightedEngagementReservationEngine(List<IWeightedEngagementReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWeightedEngagementReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WeightedEngagementReservationEngine DefaultWeightedEngagementReservationEngine(Registry flags)
    {
        return new WeightedEngagementReservationEngine(
            new List<IWeightedEngagementReservationStep>
            {
                new FlaggedWeightedEngagementReservationAdjuster(
                    flags,
                    "weighted-engagement-reservation-scheduled",
                    1.0),
                new CountryWeightedEngagementReservationAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.7, ["north"] = 1.3 }),
                new RoundingWeightedEngagementReservationAdjuster(5.0),
                new MinimumWeightedEngagementReservationAdjuster(15.2),
                new CapWeightedEngagementReservationAdjuster(41388.0),
                new PercentWeightedEngagementReservationAdjuster(-14.7),
            },
            new List<string>(),
            4.9,
            72790.0);
    }

    public double Evaluate(WeightedEngagementReservation item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<WeightedEngagementReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
