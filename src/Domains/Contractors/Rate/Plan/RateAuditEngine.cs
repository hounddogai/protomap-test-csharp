namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the rate audit calculation.</summary>
public interface IRateAuditFactor
{
    string Describe();
    double Adjust(RateAudit item, double valueValue);
}

public sealed record RateAuditRateOffsetAdjuster(double Weight) : IRateAuditFactor
{
    public string Describe()
    {
        return $"plus rate times {Weight}";
    }

    public double Adjust(RateAudit item, double valueValue)
    {
        return valueValue + item.Rate * Weight;
    }
}

public sealed record FlaggedRateAuditAdjuster(Registry Flags, string Flag, double Percent) : IRateAuditFactor
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(RateAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record MinimumRateAuditAdjuster(double Minimum) : IRateAuditFactor
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(RateAudit item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record RoundingRateAuditAdjuster(double Step) : IRateAuditFactor
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(RateAudit item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record CapRateAuditAdjuster(double Cap) : IRateAuditFactor
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(RateAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

/// <summary>Runs the rate audit steps in order and keeps a trace of each.</summary>
public sealed class RateAuditEngine(List<IRateAuditFactor> steps, List<string> trace, double floor, double ceiling)
{
    public List<IRateAuditFactor> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static RateAuditEngine DefaultRateAuditEngine(Registry flags)
    {
        return new RateAuditEngine(
            new List<IRateAuditFactor>
            {
                new RateAuditRateOffsetAdjuster(0.24),
                new FlaggedRateAuditAdjuster(flags, "rate-audit-regional", -6.7),
                new MinimumRateAuditAdjuster(32.1),
                new RoundingRateAuditAdjuster(0.25),
                new CapRateAuditAdjuster(24373.0),
            },
            new List<string>(),
            2.5,
            34164.6);
    }

    public double Price(RateAudit item)
    {
        var current = item.NetVolume();
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

    public Dictionary<string, double> EvaluateAll(List<RateAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
