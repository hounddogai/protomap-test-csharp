namespace Acme.Workforce.Workplace.AccessBadge.Threshold;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the access badge reservation calculation.</summary>
public interface IAccessBadgeReservationStep
{
    string Describe();
    double Adjust(AccessBadgeReservation item, double valueValue);
}

public sealed record CategoryAccessBadgeReservationAdjuster(Dictionary<string, double> Factors) : IAccessBadgeReservationStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(AccessBadgeReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingAccessBadgeReservationAdjuster(double Step) : IAccessBadgeReservationStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(AccessBadgeReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumAccessBadgeReservationAdjuster(double Minimum) : IAccessBadgeReservationStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(AccessBadgeReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record PercentAccessBadgeReservationAdjuster(double Percent) : IAccessBadgeReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AccessBadgeReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the access badge reservation steps in order and keeps a trace of each.</summary>
public sealed class AccessBadgeReservationEngine(List<IAccessBadgeReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAccessBadgeReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AccessBadgeReservationEngine DefaultAccessBadgeReservationEngine()
    {
        return new AccessBadgeReservationEngine(
            new List<IAccessBadgeReservationStep>
            {
                new CategoryAccessBadgeReservationAdjuster(new Dictionary<string, double> { ["north"] = 0.94 }),
                new RoundingAccessBadgeReservationAdjuster(0.05),
                new MinimumAccessBadgeReservationAdjuster(24.1),
                new PercentAccessBadgeReservationAdjuster(21.7),
            },
            new List<string>(),
            3.2,
            74635.6);
    }

    public double Compute(AccessBadgeReservation item)
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

    public Dictionary<string, double> EvaluateAll(List<AccessBadgeReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
