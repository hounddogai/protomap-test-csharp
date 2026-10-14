namespace Acme.Workforce.Absence.Holiday.Metric;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy holiday group calculation.</summary>
public interface ILegacyHolidayGroupModifier
{
    string Describe();
    double Adjust(LegacyHolidayGroup item, double valueValue);
}

public sealed record ChannelLegacyHolidayGroupAdjuster(Dictionary<string, double> Factors) : ILegacyHolidayGroupModifier
{
    public string Describe()
    {
        return "by channel";
    }

    public double Adjust(LegacyHolidayGroup item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record FlaggedLegacyHolidayGroupAdjuster(Registry Flags, string Flag, double Percent) : ILegacyHolidayGroupModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyHolidayGroup item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapLegacyHolidayGroupAdjuster(double Cap) : ILegacyHolidayGroupModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyHolidayGroup item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyHolidayGroupBatchSizeOffsetAdjuster(double Weight) : ILegacyHolidayGroupModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(LegacyHolidayGroup item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record TieredLegacyHolidayGroupAdjuster(List<double> Bounds, List<double> Rates) : ILegacyHolidayGroupModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyHolidayGroup item, double valueValue)
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

public sealed record PercentLegacyHolidayGroupAdjuster(double Percent) : ILegacyHolidayGroupModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyHolidayGroup item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy holiday group steps in order and keeps a trace of each.</summary>
public sealed class LegacyHolidayGroupEngine(List<ILegacyHolidayGroupModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyHolidayGroupModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyHolidayGroupEngine DefaultLegacyHolidayGroupEngine(Registry flags)
    {
        return new LegacyHolidayGroupEngine(
            new List<ILegacyHolidayGroupModifier>
            {
                new ChannelLegacyHolidayGroupAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.21, ["north"] = 1.05 }),
                new FlaggedLegacyHolidayGroupAdjuster(flags, "legacy-holiday-group-capped", 6.4),
                new CapLegacyHolidayGroupAdjuster(30197.0),
                new LegacyHolidayGroupBatchSizeOffsetAdjuster(1.41),
                new TieredLegacyHolidayGroupAdjuster(
                    new List<double> { 449.0, 6463.0, 8337.0 },
                    new List<double> { 0.82, 0.98, 1.0 }),
                new PercentLegacyHolidayGroupAdjuster(-21.2),
            },
            new List<string>(),
            2.2,
            58062.3);
    }

    public double Price(LegacyHolidayGroup item)
    {
        var current = item.WeightedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyHolidayGroup> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
