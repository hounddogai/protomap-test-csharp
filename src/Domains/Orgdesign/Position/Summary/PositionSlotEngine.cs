namespace Acme.Workforce.Orgdesign.Position.Summary;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the position slot calculation.</summary>
public interface IPositionSlotModifier
{
    string Describe();
    double Adjust(PositionSlot item, double valueValue);
}

public sealed record FlaggedPositionSlotAdjuster(Registry Flags, string Flag, double Percent) : IPositionSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PositionSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CapPositionSlotAdjuster(double Cap) : IPositionSlotModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PositionSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PositionSlotCapacityOffsetAdjuster(double Weight) : IPositionSlotModifier
{
    public string Describe()
    {
        return $"plus capacity times {Weight}";
    }

    public double Adjust(PositionSlot item, double valueValue)
    {
        return valueValue + (double)item.Capacity * Weight;
    }
}

/// <summary>Runs the position slot steps in order and keeps a trace of each.</summary>
public sealed class PositionSlotEngine(List<IPositionSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPositionSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PositionSlotEngine DefaultPositionSlotEngine(Registry flags)
    {
        return new PositionSlotEngine(
            new List<IPositionSlotModifier>
            {
                new FlaggedPositionSlotAdjuster(flags, "position-slot-scheduled", 1.5),
                new CapPositionSlotAdjuster(11496.0),
                new PositionSlotCapacityOffsetAdjuster(1.55),
            },
            new List<string>(),
            3.6,
            31414.0);
    }

    public double Evaluate(PositionSlot item)
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

    public Dictionary<string, double> EvaluateAll(List<PositionSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
