namespace Acme.Workforce.Absence.LeaveRequest.Snapshot;

using Acme.Workforce.Kit.Flags;

/// <summary>One step of the legacy leave request audit calculation.</summary>
public interface ILegacyLeaveRequestAuditStep
{
    string Describe();
    double Adjust(LegacyLeaveRequestAudit item, double valueValue);
}

public sealed record CapLegacyLeaveRequestAuditAdjuster(double Cap) : ILegacyLeaveRequestAuditStep
{
    public string Describe()
    {
        return $"cap at {Cap}";
    }

    public double Adjust(LegacyLeaveRequestAudit item, double valueValue)
    {
        return Math.Min(valueValue, Cap);
    }
}

public sealed record FlaggedLegacyLeaveRequestAuditAdjuster(Registry Flags, string Flag, double Percent) : ILegacyLeaveRequestAuditStep
{
    public string Describe()
    {
        return $"{Percent} when {Flag} is on";
    }

    public double Adjust(LegacyLeaveRequestAudit item, double valueValue)
    {
        if (!Flags.Enabled(Flag, item.Code))
        {
            return valueValue;
        }
        return valueValue * (1.0 + Percent / 100.0);
    }
}

public sealed record LabelLegacyLeaveRequestAuditAdjuster(Dictionary<string, double> Factors) : ILegacyLeaveRequestAuditStep
{
    public string Describe()
    {
        return "by label";
    }

    public double Adjust(LegacyLeaveRequestAudit item, double valueValue)
    {
        if (!Factors.TryGetValue(item.GroupKey(), out var factor))
        {
            return valueValue;
        }
        return valueValue * factor;
    }
}

public sealed record PercentLegacyLeaveRequestAuditAdjuster(double Percent) : ILegacyLeaveRequestAuditStep
{
    public string Describe()
    {
        return $"{Percent}% change";
    }

    public double Adjust(LegacyLeaveRequestAudit item, double valueValue)
    {
        return valueValue * (1.0 + Percent / 100.0);
    }
}

/// <summary>Runs the legacy leave request audit steps in order and keeps a trace of each.</summary>
public sealed class LegacyLeaveRequestAuditEngine(List<ILegacyLeaveRequestAuditStep> steps, List<string> trace, double floor, double ceiling)
{
    public List<ILegacyLeaveRequestAuditStep> Steps { get; set; } = steps;
    public List<string> Trace { get; set; } = trace;
    public double Floor { get; set; } = floor;
    public double Ceiling { get; set; } = ceiling;

    public static LegacyLeaveRequestAuditEngine DefaultLegacyLeaveRequestAuditEngine(Registry flags)
    {
        return new LegacyLeaveRequestAuditEngine(
            new List<ILegacyLeaveRequestAuditStep>
            {
                new CapLegacyLeaveRequestAuditAdjuster(32842.0),
                new FlaggedLegacyLeaveRequestAuditAdjuster(flags, "legacy-leave-request-audit-seasonal", -3.2),
                new LabelLegacyLeaveRequestAuditAdjuster(
                    new Dictionary<string, double> { ["south"] = 0.96, ["north"] = 1.2 }),
                new PercentLegacyLeaveRequestAuditAdjuster(-10.1),
            },
            new List<string>(),
            0.2,
            46165.8);
    }

    public double Compute(LegacyLeaveRequestAudit item)
    {
        var current = item.MeasuredValue();
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

    public Dictionary<string, double> EvaluateAll(List<LegacyLeaveRequestAudit> items)
    {
        var results = new Dictionary<string, double>();
        foreach (var item in items)
        {
            results[item.Code] = Compute(item);
        }
        return results;
    }
}
