namespace Acme.Workforce.Compliance.Audit.AuditManifest;

using Acme.Workforce.Kit.Flags;
using Acme.Workforce.Kit.Numbers;

/// <summary>One step of the capped audit reservation calculation.</summary>
public interface ICappedAuditReservationModifier
{
    string Describe();
    double Adjust(CappedAuditReservation item, double valueValue);
}

public sealed record CapCappedAuditReservationAdjuster(double Cap) : ICappedAuditReservationModifier
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(CappedAuditReservation item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record CappedAuditReservationBatchSizeOffsetAdjuster(double Weight) : ICappedAuditReservationModifier
{
    public string Describe()
    {
        return $"plus batch size times {Weight}";
    }

    public double Adjust(CappedAuditReservation item, double valueValue)
    {
        return valueValue + (double)item.BatchSize * Weight;
    }
}

public sealed record PercentCappedAuditReservationAdjuster(double Percent) : ICappedAuditReservationModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(CappedAuditReservation item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record RoundingCappedAuditReservationAdjuster(double Step) : ICappedAuditReservationModifier
{
    public string Describe()
    {
        return $"round to {Step}";
    }

    public double Adjust(CappedAuditReservation item, double valueValue)
    {
        return Numbers.RoundTo(valueValue, Step);
    }
}

public sealed record FlaggedCappedAuditReservationAdjuster(Registry Flags, string Flag, double Percent) : ICappedAuditReservationModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(CappedAuditReservation item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record DepartmentCappedAuditReservationAdjuster(Dictionary<string, double> Factors) : ICappedAuditReservationModifier
{
    public string Describe()
    {
        return "by department";
    }

    public double Adjust(CappedAuditReservation item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the capped audit reservation steps in order and keeps a trace of each.</summary>
public sealed class CappedAuditReservationEngine(List<ICappedAuditReservationModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<ICappedAuditReservationModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static CappedAuditReservationEngine DefaultCappedAuditReservationEngine(Registry flags)
    {
        return new CappedAuditReservationEngine(
            new List<ICappedAuditReservationModifier>
            {
                new CapCappedAuditReservationAdjuster(16738.0),
                new CappedAuditReservationBatchSizeOffsetAdjuster(0.73),
                new PercentCappedAuditReservationAdjuster(0.5),
                new RoundingCappedAuditReservationAdjuster(0.1),
                new FlaggedCappedAuditReservationAdjuster(flags, "capped-audit-reservation-tiered", 1.7),
                new DepartmentCappedAuditReservationAdjuster(
                    new Dictionary<string, double> { ["south"] = 1.01, ["retail"] = 0.78 }),
            },
            new List<string>(),
            1.9,
            58666.1);
    }

    public double Run(CappedAuditReservation item)
    {
        var current = item.AdjustedVolume();
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

    public Dictionary<string, double> EvaluateAll(List<CappedAuditReservation> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Run(item);
        }
        return results;
    }
}
