namespace Acme.Workforce.Assets.LicenseSeat.SeatAssignment;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the license seat quota calculation.</summary>
public interface ILicenseSeatQuotaModifier
{
    string Describe();
    double Adjust(LicenseSeatQuota item, double valueValue);
}

public sealed record TieredLicenseSeatQuotaAdjuster(List<double> Bounds, List<double> Rates) : ILicenseSeatQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(LicenseSeatQuota item, double valueValue)
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

public sealed record CapLicenseSeatQuotaAdjuster(double Cap) : ILicenseSeatQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LicenseSeatQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLicenseSeatQuotaAdjuster(Registry Flags, string Flag, double Percent) : ILicenseSeatQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LicenseSeatQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the license seat quota steps in order and keeps a trace of each.</summary>
public sealed class LicenseSeatQuotaEngine(List<ILicenseSeatQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILicenseSeatQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LicenseSeatQuotaEngine DefaultLicenseSeatQuotaEngine(Registry flags)
    {
        return new LicenseSeatQuotaEngine(
            new List<ILicenseSeatQuotaModifier>
            {
                new TieredLicenseSeatQuotaAdjuster(
                    new List<double> { 4292.0, 4800.0 },
                    new List<double> { 1.09, 1.13 }),
                new CapLicenseSeatQuotaAdjuster(10471.0),
                new FlaggedLicenseSeatQuotaAdjuster(flags, "license-seat-quota-rolling", -9.2),
            },
            new List<string>(),
            4.8,
            90264.1);
    }

    public double Evaluate(LicenseSeatQuota item)
    {
        var current = item.WeightedLoad();
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

    public Dictionary<string, double> EvaluateAll(List<LicenseSeatQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
