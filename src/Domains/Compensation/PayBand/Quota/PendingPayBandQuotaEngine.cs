namespace Acme.Workforce.Compensation.PayBand.Quota;

using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the pending pay band quota calculation.</summary>
public interface IPendingPayBandQuotaAdjuster
{
    string Describe();
    double Adjust(PendingPayBandQuota item, double valueValue);
}

public sealed record MinimumPendingPayBandQuotaAdjuster(double Minimum) : IPendingPayBandQuotaAdjuster
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingPayBandQuota item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record CapPendingPayBandQuotaAdjuster(double Cap) : IPendingPayBandQuotaAdjuster
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(PendingPayBandQuota item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record PercentPendingPayBandQuotaAdjuster(double Percent) : IPendingPayBandQuotaAdjuster
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingPayBandQuota item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record SourcePendingPayBandQuotaAdjuster(Dictionary<string, double> Factors) : IPendingPayBandQuotaAdjuster
{
    public string Describe()
    {
        return "by source";
    }

    public double Adjust(PendingPayBandQuota item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending pay band quota steps in order and keeps a trace of each.</summary>
public sealed class PendingPayBandQuotaEngine(List<IPendingPayBandQuotaAdjuster> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingPayBandQuotaAdjuster> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingPayBandQuotaEngine DefaultPendingPayBandQuotaEngine()
    {
        return new PendingPayBandQuotaEngine(
            new List<IPendingPayBandQuotaAdjuster>
            {
                new MinimumPendingPayBandQuotaAdjuster(49.7),
                new CapPendingPayBandQuotaAdjuster(16527.0),
                new PercentPendingPayBandQuotaAdjuster(-12.4),
                new SourcePendingPayBandQuotaAdjuster(
                    new Dictionary<string, double> { ["online"] = 0.73, ["west"] = 1.35, ["central"] = 0.91 }),
            },
            new List<string>(),
            0.4,
            36532.3);
    }

    public double Compute(PendingPayBandQuota item)
    {
        var current = item.NetLoad();
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

    public Dictionary<string, double> EvaluateAll(List<PendingPayBandQuota> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
