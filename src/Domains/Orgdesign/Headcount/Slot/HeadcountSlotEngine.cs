namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the headcount slot calculation.</summary>
public interface IHeadcountSlotFactor
{
    string Describe();
    double Adjust(HeadcountSlot item, double valueValue);
}

public sealed record HeadcountSlotMaximumOffsetAdjuster(double Weight) : IHeadcountSlotFactor
{
    public string Describe()
    {
        return $"plus maximum times {Weight}";
    }

    public double Adjust(HeadcountSlot item, double valueValue)
    {
        return valueValue + item.Maximum * Weight;
    }
}

public sealed record PercentHeadcountSlotAdjuster(double Percent) : IHeadcountSlotFactor
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(HeadcountSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumHeadcountSlotAdjuster(double Minimum) : IHeadcountSlotFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(HeadcountSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapHeadcountSlotAdjuster(double Cap) : IHeadcountSlotFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(HeadcountSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record TieredHeadcountSlotAdjuster(List<double> Bounds, List<double> Rates) : IHeadcountSlotFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(HeadcountSlot item, double valueValue)
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

public sealed record RoundingHeadcountSlotAdjuster(double Step) : IHeadcountSlotFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(HeadcountSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the headcount slot steps in order and keeps a trace of each.</summary>
public sealed class HeadcountSlotEngine(List<IHeadcountSlotFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IHeadcountSlotFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static HeadcountSlotEngine DefaultHeadcountSlotEngine()
    {
        return new HeadcountSlotEngine(
            new List<IHeadcountSlotFactor>
            {
                new HeadcountSlotMaximumOffsetAdjuster(0.29),
                new PercentHeadcountSlotAdjuster(-19.6),
                new MinimumHeadcountSlotAdjuster(33.2),
                new CapHeadcountSlotAdjuster(3203.0),
                new TieredHeadcountSlotAdjuster(
                    new List<double> { 3904.0, 5357.0, 7696.0, 9982.0 },
                    new List<double> { 1.11, 1.19, 0.81, 0.84 }),
                new RoundingHeadcountSlotAdjuster(0.5),
            },
            new List<string>(),
            3.7,
            15786.0);
    }

    public double Price(HeadcountSlot item)
    {
        var current = item.EffectiveTotal();
        Trace = new List<string>();
        foreach (var step in Steps)
        {
            var next = step.Adjust(item, current);
            Trace.Add($"{step.Describe()}: {Math.Round(current, 2)} -> {Math.Round(next, 2)}");
            current = next;
        }
        return Numbers.ClampFloat(current, Floor, Ceiling);
    }

    public string Explain()
    {
        return string.Join("\n", Trace);
    }

    public Dictionary<string, double> EvaluateAll(List<HeadcountSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
