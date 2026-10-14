namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled span digest is in its life cycle.</summary>
public enum ScheduledSpanDigestStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A scheduled span digest as the span team keeps it.</summary>
public sealed class ScheduledSpanDigest(string code, string label, int endsAt, string externalRef, int version, string employeeId, string unit, double ratio, int startsAt, ScheduledSpanDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string ExternalRef { get; set; } = externalRef;
    public int Version { get; set; } = version;
    public string EmployeeId { get; set; } = employeeId;
    public string Unit { get; set; } = unit;
    public double Ratio { get; set; } = ratio;
    public int StartsAt { get; set; } = startsAt;
    public ScheduledSpanDigestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled span digest in status idle.</summary>
    public static ScheduledSpanDigest NewScheduledSpanDigest(string code, string label)
    {
        return new ScheduledSpanDigest(code, label, 0, "", 0, "", "", 0.0, 0, ScheduledSpanDigestStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledSpanDigestStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledSpanDigestStatus next)
    {
        switch (Status)
        {
            case ScheduledSpanDigestStatus.Idle:
                switch (next)
                {
                    case ScheduledSpanDigestStatus.Ready:
                    case ScheduledSpanDigestStatus.Draining:
                        return true;
                }
                return false;
            case ScheduledSpanDigestStatus.Warming:
                return next == ScheduledSpanDigestStatus.Ready || next == ScheduledSpanDigestStatus.Draining;
            case ScheduledSpanDigestStatus.Ready:
                switch (next)
                {
                    case ScheduledSpanDigestStatus.Stopped:
                        return true;
                }
                return false;
            case ScheduledSpanDigestStatus.Draining:
                switch (next)
                {
                    case ScheduledSpanDigestStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledSpanDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled span digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled span digest"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)StartsAt + (double)Version, 2);
    }

    public ScheduledSpanDigest Copy()
    {
        return new ScheduledSpanDigest(
            Code,
            Label,
            EndsAt,
            ExternalRef,
            Version,
            EmployeeId,
            Unit,
            Ratio,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
