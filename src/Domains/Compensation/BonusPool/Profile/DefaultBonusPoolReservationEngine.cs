namespace Acme.Workforce.Compensation.BonusPool.Profile;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the default bonus pool reservation calculation.</summary>
public interface IDefaultBonusPoolReservationFactor
{
    string Describe();
    double Adjust(DefaultBonusPoolReservation item, double valueValue);
}

public sealed record CapDefaultBonusPoolReservationAdjuster(double Cap) : IDefaultBonusPoolReservationFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DefaultBonusPoolReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record UnitDefaultBonusPoolReservationAdjuster(Dictionary<string, double> Factors) : IDefaultBonusPoolReservationFactor
{
    public string Describe()
    {
        return "by unit";
    }

    public double Adjust(DefaultBonusPoolReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record MinimumDefaultBonusPoolReservationAdjuster(double Minimum) : IDefaultBonusPoolReservationFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(DefaultBonusPoolReservation item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedDefaultBonusPoolReservationAdjuster(Registry Flags, string Flag, double Percent) : IDefaultBonusPoolReservationFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DefaultBonusPoolReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the default bonus pool reservation steps in order and keeps a trace of each.</summary>
public sealed class DefaultBonusPoolReservationEngine(List<IDefaultBonusPoolReservationFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDefaultBonusPoolReservationFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DefaultBonusPoolReservationEngine DefaultDefaultBonusPoolReservationEngine(Registry flags)
    {
        return new DefaultBonusPoolReservationEngine(
            new List<IDefaultBonusPoolReservationFactor>
            {
                new CapDefaultBonusPoolReservationAdjuster(14620.0),
                new UnitDefaultBonusPoolReservationAdjuster(
                    new Dictionary<string, double> { ["north"] = 0.92, ["east"] = 1.35 }),
                new MinimumDefaultBonusPoolReservationAdjuster(39.0),
                new FlaggedDefaultBonusPoolReservationAdjuster(flags, "default-bonus-pool-reservation-pending", 6.1),
            },
            new List<string>(),
            1.6,
            4387.4);
    }

    public double Run(DefaultBonusPoolReservation item)
    {
        var current = item.ProjectedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<DefaultBonusPoolReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
