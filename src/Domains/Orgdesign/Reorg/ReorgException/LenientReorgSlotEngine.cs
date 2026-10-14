namespace Acme.Workforce.Orgdesign.Reorg.ReorgException;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient reorg slot calculation.</summary>
public interface ILenientReorgSlotModifier
{
    string Describe();
    double Adjust(LenientReorgSlot item, double valueValue);
}

public sealed record TieredLenientReorgSlotAdjuster(List<double> Bounds, List<double> Rates) : ILenientReorgSlotModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientReorgSlot item, double valueValue)
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

public sealed record RoundingLenientReorgSlotAdjuster(double Step) : ILenientReorgSlotModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientReorgSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record MinimumLenientReorgSlotAdjuster(double Minimum) : ILenientReorgSlotModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LenientReorgSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record LenientReorgSlotSequenceOffsetAdjuster(double Weight) : ILenientReorgSlotModifier
{
    public string Describe()
    {
        return $"plus sequence times {Weight}";
    }

    public double Adjust(LenientReorgSlot item, double valueValue)
    {
        return valueValue + (double)item.Sequence * Weight;
    }
}

public sealed record FlaggedLenientReorgSlotAdjuster(Registry Flags, string Flag, double Percent) : ILenientReorgSlotModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientReorgSlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the lenient reorg slot steps in order and keeps a trace of each.</summary>
public sealed class LenientReorgSlotEngine(List<ILenientReorgSlotModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientReorgSlotModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientReorgSlotEngine DefaultLenientReorgSlotEngine(Registry flags)
    {
        return new LenientReorgSlotEngine(
            new List<ILenientReorgSlotModifier>
            {
                new TieredLenientReorgSlotAdjuster(
                    new List<double> { 1428.0, 4869.0, 9179.0 },
                    new List<double> { 1.02, 0.93, 0.98 }),
                new RoundingLenientReorgSlotAdjuster(1.0),
                new MinimumLenientReorgSlotAdjuster(32.9),
                new LenientReorgSlotSequenceOffsetAdjuster(1.79),
                new FlaggedLenientReorgSlotAdjuster(flags, "lenient-reorg-slot-strict", 1.2),
            },
            new List<string>(),
            1.8,
            30286.5);
    }

    public double Compute(LenientReorgSlot item)
    {
        var current = item.WeightedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LenientReorgSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
