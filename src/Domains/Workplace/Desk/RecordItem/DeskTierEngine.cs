namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the desk tier calculation.</summary>
public interface IDeskTierModifier
{
    string Describe();
    double Adjust(DeskTier item, double valueValue);
}

public sealed record CapDeskTierAdjuster(double Cap) : IDeskTierModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(DeskTier item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredDeskTierAdjuster(List<double> Bounds, List<double> Rates) : IDeskTierModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(DeskTier item, double valueValue)
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

public sealed record FlaggedDeskTierAdjuster(Registry Flags, string Flag, double Percent) : IDeskTierModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(DeskTier item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingDeskTierAdjuster(double Step) : IDeskTierModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(DeskTier item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentDeskTierAdjuster(double Percent) : IDeskTierModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(DeskTier item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TeamDeskTierAdjuster(Dictionary<string, double> Factors) : IDeskTierModifier
{
    public string Describe()
    {
        return "by team";
    }

    public double Adjust(DeskTier item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the desk tier steps in order and keeps a trace of each.</summary>
public sealed class DeskTierEngine(List<IDeskTierModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IDeskTierModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static DeskTierEngine DefaultDeskTierEngine(Registry flags)
    {
        return new DeskTierEngine(
            new List<IDeskTierModifier>
            {
                new CapDeskTierAdjuster(22045.0),
                new TieredDeskTierAdjuster(
                    new List<double> { 189.0, 983.0, 2129.0, 3799.0 },
                    new List<double> { 1.02, 0.93, 0.83, 0.94 }),
                new FlaggedDeskTierAdjuster(flags, "desk-tier-rolling", 3.4),
                new RoundingDeskTierAdjuster(0.25),
                new PercentDeskTierAdjuster(-23.4),
                new TeamDeskTierAdjuster(new Dictionary<string, double> { ["online"] = 0.96 }),
            },
            new List<string>(),
            2.8,
            16830.4);
    }

    public double Price(DeskTier item)
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

    public Dictionary<string, double> EvaluateAll(List<DeskTier> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
