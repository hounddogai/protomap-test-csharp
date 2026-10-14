namespace Acme.Workforce.Compliance.Audit.Digest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the audit reservation calculation.</summary>
public interface IAuditReservationStep
{
    string Describe();
    double Adjust(AuditReservation item, double valueValue);
}

public sealed record TitleAuditReservationAdjuster(Dictionary<string, double> Factors) : IAuditReservationStep
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(AuditReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record TieredAuditReservationAdjuster(List<double> Bounds, List<double> Rates) : IAuditReservationStep
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(AuditReservation item, double valueValue)
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

public sealed record CapAuditReservationAdjuster(double Cap) : IAuditReservationStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(AuditReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedAuditReservationAdjuster(Registry Flags, string Flag, double Percent) : IAuditReservationStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(AuditReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PercentAuditReservationAdjuster(double Percent) : IAuditReservationStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(AuditReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the audit reservation steps in order and keeps a trace of each.</summary>
public sealed class AuditReservationEngine(List<IAuditReservationStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<IAuditReservationStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static AuditReservationEngine DefaultAuditReservationEngine(Registry flags)
    {
        return new AuditReservationEngine(
            new List<IAuditReservationStep>
            {
                new TitleAuditReservationAdjuster(
                    new Dictionary<string, double> { ["online"] = 1.13, ["west"] = 1.14, ["central"] = 0.86 }),
                new TieredAuditReservationAdjuster(
                    new List<double> { 147.0, 281.0, 4467.0, 6762.0 },
                    new List<double> { 0.87, 0.91, 1.04, 0.86 }),
                new CapAuditReservationAdjuster(23932.0),
                new FlaggedAuditReservationAdjuster(flags, "audit-reservation-weighted", 2.5),
                new PercentAuditReservationAdjuster(-4.0),
            },
            new List<string>(),
            3.8,
            32178.1);
    }

    public double Price(AuditReservation item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<AuditReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Price(item);
        }
        return results;
    }
}
