namespace Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy occupancy record calculation.</summary>
public interface ILegacyOccupancyRecordStep
{
    string Describe();
    double Adjust(LegacyOccupancyRecord item, double valueValue);
}

public sealed record PercentLegacyOccupancyRecordAdjuster(double Percent) : ILegacyOccupancyRecordStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyOccupancyRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredLegacyOccupancyRecordAdjuster(List<double> Bounds, List<double> Rates) : ILegacyOccupancyRecordStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyOccupancyRecord item, double valueValue)
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

public sealed record MinimumLegacyOccupancyRecordAdjuster(double Minimum) : ILegacyOccupancyRecordStep
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyOccupancyRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CategoryLegacyOccupancyRecordAdjuster(Dictionary<string, double> Factors) : ILegacyOccupancyRecordStep
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(LegacyOccupancyRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLegacyOccupancyRecordAdjuster(double Step) : ILegacyOccupancyRecordStep
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LegacyOccupancyRecord item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapLegacyOccupancyRecordAdjuster(double Cap) : ILegacyOccupancyRecordStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyOccupancyRecord item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy occupancy record steps in order and keeps a trace of each.</summary>
public sealed class LegacyOccupancyRecordEngine(List<ILegacyOccupancyRecordStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyOccupancyRecordStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyOccupancyRecordEngine DefaultLegacyOccupancyRecordEngine()
    {
        return new LegacyOccupancyRecordEngine(
            new List<ILegacyOccupancyRecordStep>
            {
                new PercentLegacyOccupancyRecordAdjuster(17.6),
                new TieredLegacyOccupancyRecordAdjuster(
                    new List<double> { 986.0, 4767.0, 6380.0, 8531.0 },
                    new List<double> { 0.88, 0.96, 0.81, 1.1 }),
                new MinimumLegacyOccupancyRecordAdjuster(38.5),
                new CategoryLegacyOccupancyRecordAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.76, ["east"] = 1.17 }),
                new RoundingLegacyOccupancyRecordAdjuster(5.0),
                new CapLegacyOccupancyRecordAdjuster(27335.0),
            },
            new List<string>(),
            2.9,
            33996.0);
    }

    public double Run(LegacyOccupancyRecord item)
    {
        var current = item.EffectiveValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyOccupancyRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
