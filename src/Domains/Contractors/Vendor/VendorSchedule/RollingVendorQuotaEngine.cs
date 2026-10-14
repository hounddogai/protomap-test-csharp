namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rolling vendor quota calculation.</summary>
public interface IRollingVendorQuotaModifier
{
    string Describe();
    double Adjust(RollingVendorQuota item, double valueValue);
}

public sealed record PercentRollingVendorQuotaAdjuster(double Percent) : IRollingVendorQuotaModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(RollingVendorQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record TieredRollingVendorQuotaAdjuster(List<double> Bounds, List<double> Rates) : IRollingVendorQuotaModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(RollingVendorQuota item, double valueValue)
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

public sealed record CapRollingVendorQuotaAdjuster(double Cap) : IRollingVendorQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RollingVendorQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record RoundingRollingVendorQuotaAdjuster(double Step) : IRollingVendorQuotaModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RollingVendorQuota item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

/// <summary>Runs the rolling vendor quota steps in order and keeps a trace of each.</summary>
public sealed class RollingVendorQuotaEngine(List<IRollingVendorQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRollingVendorQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RollingVendorQuotaEngine DefaultRollingVendorQuotaEngine()
    {
        return new RollingVendorQuotaEngine(
            new List<IRollingVendorQuotaModifier>
            {
                new PercentRollingVendorQuotaAdjuster(-17.0),
                new TieredRollingVendorQuotaAdjuster(
                    new List<double> { 5540.0, 8654.0 },
                    new List<double> { 0.89, 1.14 }),
                new CapRollingVendorQuotaAdjuster(31690.0),
                new RoundingRollingVendorQuotaAdjuster(0.25),
            },
            new List<string>(),
            2.1,
            13099.0);
    }

    public double Evaluate(RollingVendorQuota item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RollingVendorQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Evaluate(item);
        }
        return results;
    }
}
