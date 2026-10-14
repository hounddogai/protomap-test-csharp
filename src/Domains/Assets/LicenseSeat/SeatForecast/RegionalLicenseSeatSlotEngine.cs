namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the regional license seat slot calculation.</summary>
public interface IRegionalLicenseSeatSlotAdjuster
{
    string Describe();
    double Adjust(RegionalLicenseSeatSlot item, double valueValue);
}

public sealed record RoundingRegionalLicenseSeatSlotAdjuster(double Step) : IRegionalLicenseSeatSlotAdjuster
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RegionalLicenseSeatSlot item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredRegionalLicenseSeatSlotAdjuster(List<double> Bounds, List<double> Rates) : IRegionalLicenseSeatSlotAdjuster
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RegionalLicenseSeatSlot item, double valueValue)
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

public sealed record CategoryRegionalLicenseSeatSlotAdjuster(Dictionary<string, double> Factors) : IRegionalLicenseSeatSlotAdjuster
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(RegionalLicenseSeatSlot item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the regional license seat slot steps in order and keeps a trace of each.</summary>
public sealed class RegionalLicenseSeatSlotEngine(List<IRegionalLicenseSeatSlotAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRegionalLicenseSeatSlotAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RegionalLicenseSeatSlotEngine DefaultRegionalLicenseSeatSlotEngine()
    {
        return new RegionalLicenseSeatSlotEngine(
            new List<IRegionalLicenseSeatSlotAdjuster>
            {
                new RoundingRegionalLicenseSeatSlotAdjuster(0.1),
                new TieredRegionalLicenseSeatSlotAdjuster(
                    new List<double> { 6037.0, 9879.0 },
                    new List<double> { 0.86, 1.03 }),
                new CategoryRegionalLicenseSeatSlotAdjuster(
                    new Dictionary<string, double> { ["central"] = 0.78, ["south"] = 1.04 }),
            },
            new List<string>(),
            0.3,
            74875.7);
    }

    public double Compute(RegionalLicenseSeatSlot item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RegionalLicenseSeatSlot> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
