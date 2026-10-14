namespace Acme.Workforce.Engagement.TownHall.BatchedHallRule;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the town hall group calculation.</summary>
public interface ITownHallGroupStep
{
    string Describe();
    double Adjust(TownHallGroup item, double valueValue);
}

public sealed record TownHallGroupTenureMonthsOffsetAdjuster(double Weight) : ITownHallGroupStep
{
    public string Describe()
    {
        return $"plus tenure months times {Weight}";
    }

    public double Adjust(TownHallGroup item, double valueValue)
    {
        return valueValue + (double)item.TenureMonths * Weight;
    }
}

public sealed record TieredTownHallGroupAdjuster(List<double> Bounds, List<double> Rates) : ITownHallGroupStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(TownHallGroup item, double valueValue)
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

public sealed record CapTownHallGroupAdjuster(double Cap) : ITownHallGroupStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(TownHallGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingTownHallGroupAdjuster(double Step) : ITownHallGroupStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(TownHallGroup item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumTownHallGroupAdjuster(double Minimum) : ITownHallGroupStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(TownHallGroup item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record FlaggedTownHallGroupAdjuster(Registry Flags, string Flag, double Percent) : ITownHallGroupStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(TownHallGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the town hall group steps in order and keeps a trace of each.</summary>
public sealed class TownHallGroupEngine(List<ITownHallGroupStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ITownHallGroupStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static TownHallGroupEngine DefaultTownHallGroupEngine(Registry flags)
    {
        return new TownHallGroupEngine(
            new List<ITownHallGroupStep>
            {
                new TownHallGroupTenureMonthsOffsetAdjuster(0.12),
                new TieredTownHallGroupAdjuster(
                    new List<double> { 2110.0, 5922.0, 7665.0, 7820.0 },
                    new List<double> { 0.93, 1.18, 0.87, 1.0 }),
                new CapTownHallGroupAdjuster(11030.0),
                new RoundingTownHallGroupAdjuster(0.1),
                new MinimumTownHallGroupAdjuster(44.4),
                new FlaggedTownHallGroupAdjuster(flags, "town-hall-group-pending", 2.9),
            },
            new List<string>(),
            3.6,
            20093.8);
    }

    public double Compute(TownHallGroup item)
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

    public Dictionary<string, double> EvaluateAll(List<TownHallGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
