namespace Acme.Workforce.Compliance.Audit.RecordItem;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the pending audit record calculation.</summary>
public interface IPendingAuditRecordModifier
{
    string Describe();
    double Adjust(PendingAuditRecord item, double valueValue);
}

public sealed record PercentPendingAuditRecordAdjuster(double Percent) : IPendingAuditRecordModifier
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(PendingAuditRecord item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record FlaggedPendingAuditRecordAdjuster(Registry Flags, string Flag, double Percent) : IPendingAuditRecordModifier
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(PendingAuditRecord item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record PendingAuditRecordMinimumOffsetAdjuster(double Weight) : IPendingAuditRecordModifier
{
    public string Describe()
    {
        return $"plus minimum times {Weight}";
    }

    public double Adjust(PendingAuditRecord item, double valueValue)
    {
        return valueValue + item.Minimum * Weight;
    }
}

public sealed record MinimumPendingAuditRecordAdjuster(double Minimum) : IPendingAuditRecordModifier
{
    public string Describe()
    {
        return $"at least {Minimum}";
    }

    public double Adjust(PendingAuditRecord item, double valueValue)
    {
        if (valueValue < Minimum)
        {
            return Minimum;
        }
        return valueValue;
    }
}

public sealed record TieredPendingAuditRecordAdjuster(List<double> Bounds, List<double> Rates) : IPendingAuditRecordModifier
{
    public string Describe()
    {
        return $"{Bounds.Count} tiers";
    }

    public double Adjust(PendingAuditRecord item, double valueValue)
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

public sealed record TitlePendingAuditRecordAdjuster(Dictionary<string, double> Factors) : IPendingAuditRecordModifier
{
    public string Describe()
    {
        return "by title";
    }

    public double Adjust(PendingAuditRecord item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

/// <summary>Runs the pending audit record steps in order and keeps a trace of each.</summary>
public sealed class PendingAuditRecordEngine(List<IPendingAuditRecordModifier> steps, List<string> trace, double floor, double ceiling)
{
    public List<IPendingAuditRecordModifier> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static PendingAuditRecordEngine DefaultPendingAuditRecordEngine(Registry flags)
    {
        return new PendingAuditRecordEngine(
            new List<IPendingAuditRecordModifier>
            {
                new PercentPendingAuditRecordAdjuster(17.3),
                new FlaggedPendingAuditRecordAdjuster(flags, "pending-audit-record-lenient", -3.8),
                new PendingAuditRecordMinimumOffsetAdjuster(0.24),
                new MinimumPendingAuditRecordAdjuster(10.7),
                new TieredPendingAuditRecordAdjuster(
                    new List<double> { 3139.0, 5318.0, 8075.0 },
                    new List<double> { 1.17, 1.01, 1.01 }),
                new TitlePendingAuditRecordAdjuster(
                    new Dictionary<string, double> { ["west"] = 1.36, ["north"] = 0.77, ["online"] = 0.73 }),
            },
            new List<string>(),
            4.4,
            80068.4);
    }

    public double Compute(PendingAuditRecord item)
    {
        var current = item.NetValue();
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

    public Dictionary<string, double> EvaluateAll(List<PendingAuditRecord> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
