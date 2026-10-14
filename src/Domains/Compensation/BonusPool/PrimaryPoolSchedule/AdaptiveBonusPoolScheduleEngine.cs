namespace Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the adaptive bonus pool schedule calculation.</summary>
public interface IAdaptiveBonusPoolScheduleFactor
{
    string Describe();
    double Adjust(AdaptiveBonusPoolSchedule item, double valueValue);
}

public sealed record TieredAdaptiveBonusPoolScheduleAdjuster(List<double> Bounds, List<double> Rates) : IAdaptiveBonusPoolScheduleFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AdaptiveBonusPoolSchedule item, double valueValue)
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

public sealed record PercentAdaptiveBonusPoolScheduleAdjuster(double Percent) : IAdaptiveBonusPoolScheduleFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AdaptiveBonusPoolSchedule item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapAdaptiveBonusPoolScheduleAdjuster(double Cap) : IAdaptiveBonusPoolScheduleFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AdaptiveBonusPoolSchedule item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record AdaptiveBonusPoolScheduleLimitOffsetAdjuster(double Weight) : IAdaptiveBonusPoolScheduleFactor
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(AdaptiveBonusPoolSchedule item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record CountryAdaptiveBonusPoolScheduleAdjuster(Dictionary<string, double> Factors) : IAdaptiveBonusPoolScheduleFactor
{
    public string Describe()
    {
        return "by country";
    }

    public double Adjust(AdaptiveBonusPoolSchedule item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedAdaptiveBonusPoolScheduleAdjuster(Registry Flags, string Flag, double Percent) : IAdaptiveBonusPoolScheduleFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AdaptiveBonusPoolSchedule item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the adaptive bonus pool schedule steps in order and keeps a trace of each.</summary>
public sealed class AdaptiveBonusPoolScheduleEngine(List<IAdaptiveBonusPoolScheduleFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAdaptiveBonusPoolScheduleFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AdaptiveBonusPoolScheduleEngine DefaultAdaptiveBonusPoolScheduleEngine(Registry flags)
    {
        return new AdaptiveBonusPoolScheduleEngine(
            new List<IAdaptiveBonusPoolScheduleFactor>
            {
                new TieredAdaptiveBonusPoolScheduleAdjuster(
                    new List<double> { 3645.0, 4151.0, 6040.0, 7594.0 },
                    new List<double> { 0.8, 0.98, 1.0, 0.92 }),
                new PercentAdaptiveBonusPoolScheduleAdjuster(-1.2),
                new CapAdaptiveBonusPoolScheduleAdjuster(30535.0),
                new AdaptiveBonusPoolScheduleLimitOffsetAdjuster(1.52),
                new CountryAdaptiveBonusPoolScheduleAdjuster(
                    new Dictionary<string, double> { ["east"] = 0.77, ["south"] = 1.39, ["online"] = 1.35 }),
                new FlaggedAdaptiveBonusPoolScheduleAdjuster(flags, "adaptive-bonus-pool-schedule-rolling", -4.6),
            },
            new List<string>(),
            0.2,
            44952.0);
    }

    public double Run(AdaptiveBonusPoolSchedule item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<AdaptiveBonusPoolSchedule> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
