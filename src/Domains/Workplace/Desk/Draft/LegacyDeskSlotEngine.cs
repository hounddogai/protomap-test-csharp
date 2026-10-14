namespace Acme.Workforce.Workplace.Desk.Draft;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy desk slot calculation.</summary>
public interface ILegacyDeskSlotAdjuster
{
    string Describe();
    double Adjust(LegacyDeskSlot item, double valueValue);
}

public sealed record TieredLegacyDeskSlotAdjuster(List<double> Bounds, List<double> Rates) : ILegacyDeskSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LegacyDeskSlot item, double valueValue)
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

public sealed record MinimumLegacyDeskSlotAdjuster(double Minimum) : ILegacyDeskSlotAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(LegacyDeskSlot item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapLegacyDeskSlotAdjuster(double Cap) : ILegacyDeskSlotAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyDeskSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the legacy desk slot steps in order and keeps a trace of each.</summary>
public sealed class LegacyDeskSlotEngine(List<ILegacyDeskSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyDeskSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyDeskSlotEngine DefaultLegacyDeskSlotEngine()
    {
        return new LegacyDeskSlotEngine(
            new List<ILegacyDeskSlotAdjuster>
            {
                new TieredLegacyDeskSlotAdjuster(
                    new List<double> { 357.0, 585.0, 5816.0 },
                    new List<double> { 0.85, 0.95, 0.84 }),
                new MinimumLegacyDeskSlotAdjuster(46.0),
                new CapLegacyDeskSlotAdjuster(28585.0),
            },
            new List<string>(),
            2.5,
            18857.5);
    }

    public double Run(LegacyDeskSlot item)
    {
        var current = item.ProjectedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyDeskSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
