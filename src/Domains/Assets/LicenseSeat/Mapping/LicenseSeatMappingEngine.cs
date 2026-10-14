namespace Acme.Workforce.Assets.LicenseSeat.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat mapping calculation.</summary>
public interface ILicenseSeatMappingModifier
{
    string Describe();
    double Adjust(LicenseSeatMapping item, double valueValue);
}

public sealed record TieredLicenseSeatMappingAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatMapping item, double valueValue)
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

public sealed record CategoryLicenseSeatMappingAdjuster(Dictionary<string, double> Factors) : ILicenseSeatMappingModifier
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(LicenseSeatMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapLicenseSeatMappingAdjuster(double Cap) : ILicenseSeatMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLicenseSeatMappingAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat mapping steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatMappingEngine(List<ILicenseSeatMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatMappingEngine DefaultLicenseSeatMappingEngine(Registry flags)
    {
        return new LicenseSeatMappingEngine(
            new List<ILicenseSeatMappingModifier>
            {
                new TieredLicenseSeatMappingAdjuster(
                    new List<double> { 1692.0, 2102.0 },
                    new List<double> { 1.14, 0.98 }),
                new CategoryLicenseSeatMappingAdjuster(
                    new Dictionary<string, double> { ["retail"] = 0.75, ["south"] = 1.12, ["west"] = 1.02 }),
                new CapLicenseSeatMappingAdjuster(42642.0),
                new FlaggedLicenseSeatMappingAdjuster(flags, "license-seat-mapping-default", -7.0),
            },
            new List<string>(),
            3.4,
            28318.6);
    }

    public double Evaluate(LicenseSeatMapping item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
