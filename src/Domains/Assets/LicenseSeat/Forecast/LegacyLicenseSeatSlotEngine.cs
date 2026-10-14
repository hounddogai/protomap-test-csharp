namespace Acme.Workforce.Assets.LicenseSeat.Forecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the legacy license seat slot calculation.</summary>
public interface ILegacyLicenseSeatSlotStep
{
    string Describe();
    double Adjust(LegacyLicenseSeatSlot item, double valueValue);
}

public sealed record CapLegacyLicenseSeatSlotAdjuster(double Cap) : ILegacyLicenseSeatSlotStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyLicenseSeatSlot item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record LegacyLicenseSeatSlotWeightOffsetAdjuster(double Weight) : ILegacyLicenseSeatSlotStep
{
    public string Describe()
    {
        return $"plus weight times {Weight}";
    }

    public double Adjust(LegacyLicenseSeatSlot item, double valueValue)
    {
        return valueValue + item.Weight * Weight;
    }
}

public sealed record PercentLegacyLicenseSeatSlotAdjuster(double Percent) : ILegacyLicenseSeatSlotStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyLicenseSeatSlot item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourceLegacyLicenseSeatSlotAdjuster(Dictionary<string, double> Factors) : ILegacyLicenseSeatSlotStep
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(LegacyLicenseSeatSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the legacy license seat slot steps in order and keeps a trace of each.</summary>
public sealed class LegacyLicenseSeatSlotEngine(List<ILegacyLicenseSeatSlotStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLicenseSeatSlotStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLicenseSeatSlotEngine DefaultLegacyLicenseSeatSlotEngine()
    {
        return new LegacyLicenseSeatSlotEngine(
            new List<ILegacyLicenseSeatSlotStep>
            {
                new CapLegacyLicenseSeatSlotAdjuster(9552.0),
                new LegacyLicenseSeatSlotWeightOffsetAdjuster(1.37),
                new PercentLegacyLicenseSeatSlotAdjuster(-17.7),
                new SourceLegacyLicenseSeatSlotAdjuster(new Dictionary<string, double> { ["retail"] = 1.11 }),
            },
            new List<string>(),
            0.1,
            74410.0);
    }

    public double Compute(LegacyLicenseSeatSlot item)
    {
        var current = item.AdjustedValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLicenseSeatSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
