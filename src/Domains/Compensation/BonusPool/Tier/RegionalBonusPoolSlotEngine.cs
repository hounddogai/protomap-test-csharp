namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional bonus pool slot calculation.</summary>
public interface IRegionalBonusPoolSlotModifier
{
    string Describe();
    double Adjust(RegionalBonusPoolSlot item, double valueValue);
}

public sealed record CapRegionalBonusPoolSlotAdjuster(double Cap) : IRegionalBonusPoolSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RegionalBonusPoolSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRegionalBonusPoolSlotAdjuster(double Step) : IRegionalBonusPoolSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalBonusPoolSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record PercentRegionalBonusPoolSlotAdjuster(double Percent) : IRegionalBonusPoolSlotModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RegionalBonusPoolSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedRegionalBonusPoolSlotAdjuster(Registry Flags, string Flag, double Percent) : IRegionalBonusPoolSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RegionalBonusPoolSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRegionalBonusPoolSlotAdjuster(double Minimum) : IRegionalBonusPoolSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RegionalBonusPoolSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredRegionalBonusPoolSlotAdjuster(List<double> Bounds, List<double> Rates) : IRegionalBonusPoolSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalBonusPoolSlot item, double valueValue)
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

/// <summary>Runs the regional bonus pool slot steps in order and keeps a trace of each.</summary>
public sealed class RegionalBonusPoolSlotEngine(List<IRegionalBonusPoolSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalBonusPoolSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalBonusPoolSlotEngine DefaultRegionalBonusPoolSlotEngine(Registry flags)
    {
        return new RegionalBonusPoolSlotEngine(
            new List<IRegionalBonusPoolSlotModifier>
            {
                new CapRegionalBonusPoolSlotAdjuster(21259.0),
                new RoundingRegionalBonusPoolSlotAdjuster(0.1),
                new PercentRegionalBonusPoolSlotAdjuster(-4.7),
                new FlaggedRegionalBonusPoolSlotAdjuster(flags, "regional-bonus-pool-slot-scheduled", -1.8),
                new MinimumRegionalBonusPoolSlotAdjuster(31.8),
                new TieredRegionalBonusPoolSlotAdjuster(
                    new List<double> { 1109.0, 1184.0, 7941.0 },
                    new List<double> { 0.97, 1.02, 0.95 }),
            },
            new List<string>(),
            3.1,
            17541.5);
    }

    public double Price(RegionalBonusPoolSlot item)
    {
        var current = item.NetTotal();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalBonusPoolSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
