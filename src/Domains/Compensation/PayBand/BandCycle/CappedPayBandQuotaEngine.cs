namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped pay band quota calculation.</summary>
public interface ICappedPayBandQuotaModifier
{
    string Describe();
    double Adjust(CappedPayBandQuota item, double valueValue);
}

public sealed record DepartmentCappedPayBandQuotaAdjuster(Dictionary<string, double> Factors) : ICappedPayBandQuotaModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(CappedPayBandQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record CapCappedPayBandQuotaAdjuster(double Cap) : ICappedPayBandQuotaModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedPayBandQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedCappedPayBandQuotaAdjuster(Registry Flags, string Flag, double Percent) : ICappedPayBandQuotaModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedPayBandQuota item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record CappedPayBandQuotaFloorValueOffsetAdjuster(double Weight) : ICappedPayBandQuotaModifier
{
    public string Describe()
    {
        return $"plus floor value times {Weight}";
    }

    public double Adjust(CappedPayBandQuota item, double valueValue)
    {
        return valueValue + item.FloorValue * Weight;
    }
}

/// <summary>Runs the capped pay band quota steps in order and keeps a trace of each.</summary>
public sealed class CappedPayBandQuotaEngine(List<ICappedPayBandQuotaModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedPayBandQuotaModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedPayBandQuotaEngine DefaultCappedPayBandQuotaEngine(Registry flags)
    {
        return new CappedPayBandQuotaEngine(
            new List<ICappedPayBandQuotaModifier>
            {
                new DepartmentCappedPayBandQuotaAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.74, ["online"] = 1.29 }),
                new CapCappedPayBandQuotaAdjuster(36235.0),
                new FlaggedCappedPayBandQuotaAdjuster(flags, "capped-pay-band-quota-regional", 0.4),
                new CappedPayBandQuotaFloorValueOffsetAdjuster(1.7),
            },
            new List<string>(),
            3.1,
            84812.3);
    }

    public double Compute(CappedPayBandQuota item)
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

    public Dictionary<string, double> EvaluateAll(List<CappedPayBandQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
