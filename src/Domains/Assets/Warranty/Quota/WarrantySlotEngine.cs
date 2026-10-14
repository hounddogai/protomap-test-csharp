namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the warranty slot calculation.</summary>
public interface IWarrantySlotAdjuster
{
    string Describe();
    double Adjust(WarrantySlot item, double valueValue);
}

public sealed record WarrantySlotLimitOffsetAdjuster(double Weight) : IWarrantySlotAdjuster
{
    public string Describe()
    {
        return $"plus limit times {Weight}";
    }

    public double Adjust(WarrantySlot item, double valueValue)
    {
        return valueValue + (double)item.Limit * Weight;
    }
}

public sealed record PercentWarrantySlotAdjuster(double Percent) : IWarrantySlotAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(WarrantySlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredWarrantySlotAdjuster(List<double> Bounds, List<double> Rates) : IWarrantySlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(WarrantySlot item, double valueValue)
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

public sealed record FlaggedWarrantySlotAdjuster(Registry Flags, string Flag, double Percent) : IWarrantySlotAdjuster
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(WarrantySlot item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingWarrantySlotAdjuster(double Step) : IWarrantySlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(WarrantySlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the warranty slot steps in order and keeps a trace of each.</summary>
public sealed class WarrantySlotEngine(List<IWarrantySlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IWarrantySlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static WarrantySlotEngine DefaultWarrantySlotEngine(Registry flags)
    {
        return new WarrantySlotEngine(
            new List<IWarrantySlotAdjuster>
            {
                new WarrantySlotLimitOffsetAdjuster(0.6),
                new PercentWarrantySlotAdjuster(-18.6),
                new TieredWarrantySlotAdjuster(
                    new List<double> { 2019.0, 4387.0, 5531.0 },
                    new List<double> { 1.06, 1.16, 1.03 }),
                new FlaggedWarrantySlotAdjuster(flags, "warranty-slot-manual", -8.6),
                new RoundingWarrantySlotAdjuster(0.1),
            },
            new List<string>(),
            2.1,
            45098.8);
    }

    public double Run(WarrantySlot item)
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

    public Dictionary<string, double> EvaluateAll(List<WarrantySlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
