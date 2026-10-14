namespace Acme.Workforce.Payrollops.Garnishment.Mapping;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the lenient garnishment mapping calculation.</summary>
public interface ILenientGarnishmentMappingModifier
{
    string Describe();
    double Adjust(LenientGarnishmentMapping item, double valueValue);
}

public sealed record FlaggedLenientGarnishmentMappingAdjuster(Registry Flags, string Flag, double Percent) : ILenientGarnishmentMappingModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LenientGarnishmentMapping item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentLenientGarnishmentMappingAdjuster(Dictionary<string, double> Factors) : ILenientGarnishmentMappingModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(LenientGarnishmentMapping item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record RoundingLenientGarnishmentMappingAdjuster(double Step) : ILenientGarnishmentMappingModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LenientGarnishmentMapping item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record TieredLenientGarnishmentMappingAdjuster(List<double> Bounds, List<double> Rates) : ILenientGarnishmentMappingModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LenientGarnishmentMapping item, double valueValue)
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

public sealed record CapLenientGarnishmentMappingAdjuster(double Cap) : ILenientGarnishmentMappingModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LenientGarnishmentMapping item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the lenient garnishment mapping steps in order and keeps a trace of each.</summary>
public sealed class LenientGarnishmentMappingEngine(List<ILenientGarnishmentMappingModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILenientGarnishmentMappingModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LenientGarnishmentMappingEngine DefaultLenientGarnishmentMappingEngine(Registry flags)
    {
        return new LenientGarnishmentMappingEngine(
            new List<ILenientGarnishmentMappingModifier>
            {
                new FlaggedLenientGarnishmentMappingAdjuster(flags, "lenient-garnishment-mapping-regional", 4.1),
                new DepartmentLenientGarnishmentMappingAdjuster(
                    new Dictionary<string, double> { ["west"] = 0.77, ["east"] = 0.82, ["south"] = 1.38 }),
                new RoundingLenientGarnishmentMappingAdjuster(5.0),
                new TieredLenientGarnishmentMappingAdjuster(
                    new List<double> { 1098.0, 2362.0, 3929.0 },
                    new List<double> { 0.82, 0.95, 0.97 }),
                new CapLenientGarnishmentMappingAdjuster(36447.0),
            },
            new List<string>(),
            3.3,
            32401.5);
    }

    public double Price(LenientGarnishmentMapping item)
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

    public Dictionary<string, double> EvaluateAll(List<LenientGarnishmentMapping> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
