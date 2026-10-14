namespace Acme.Workforce.Assets.LicenseSeat.SeatMilestone;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat override calculation.</summary>
public interface ILicenseSeatOverrideFactor
{
    string Describe();
    double Adjust(LicenseSeatOverride item, double valueValue);
}

public sealed record RoundingLicenseSeatOverrideAdjuster(double Step) : ILicenseSeatOverrideFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(LicenseSeatOverride item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record LicenseSeatOverrideVersionOffsetAdjuster(double Weight) : ILicenseSeatOverrideFactor
{
    public string Describe()
    {
        return $"plus version times {Weight}";
    }

    public double Adjust(LicenseSeatOverride item, double valueValue)
    {
        return valueValue + (double)item.Version * Weight;
    }
}

public sealed record CapLicenseSeatOverrideAdjuster(double Cap) : ILicenseSeatOverrideFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatOverride item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLicenseSeatOverrideAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatOverrideFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatOverride item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CategoryLicenseSeatOverrideAdjuster(Dictionary<string, double> Factors) : ILicenseSeatOverrideFactor
{
    public string Describe()
    {
        return "by category";
    }

    public double Adjust(LicenseSeatOverride item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredLicenseSeatOverrideAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatOverrideFactor
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatOverride item, double valueValue)
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

/// <summary>Runs the license seat override steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatOverrideEngine(List<ILicenseSeatOverrideFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatOverrideFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatOverrideEngine DefaultLicenseSeatOverrideEngine(Registry flags)
    {
        return new LicenseSeatOverrideEngine(
            new List<ILicenseSeatOverrideFactor>
            {
                new RoundingLicenseSeatOverrideAdjuster(0.05),
                new LicenseSeatOverrideVersionOffsetAdjuster(0.18),
                new CapLicenseSeatOverrideAdjuster(44280.0),
                new FlaggedLicenseSeatOverrideAdjuster(flags, "license-seat-override-primary", 2.8),
                new CategoryLicenseSeatOverrideAdjuster(
                    new Dictionary<string, double> { ["central"] = 1.23, ["north"] = 1.07 }),
                new TieredLicenseSeatOverrideAdjuster(
                    new List<double> { 1114.0, 1889.0 },
                    new List<double> { 1.05, 0.98 }),
            },
            new List<string>(),
            1.4,
            33098.0);
    }

    public double Compute(LicenseSeatOverride item)
    {
        var current = item.ProjectedExposure();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatOverride> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
