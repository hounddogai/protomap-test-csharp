namespace Acme.Workforce.Compliance.Audit.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit group is in its life cycle.</summary>
public enum AuditGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a audit group and its current status.</summary>
public sealed class AuditGroup(string code, string label, string timezone, int limit, string unit, double floorValue, double rate, int capacity, AuditGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Timezone { get; set; } = timezone;
    public int Limit { get; set; } = limit;
    public string Unit { get; set; } = unit;
    public double FloorValue { get; set; } = floorValue;
    public double Rate { get; set; } = rate;
    public int Capacity { get; set; } = capacity;
    public AuditGroupStatus Status { get; set; } = status;

    /// <summary>Starts a audit group in status idle.</summary>
    public static AuditGroup NewAuditGroup(string code, string label)
    {
        return new AuditGroup(code, label, "", 0, "", 0.0, 0.0, 0, AuditGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditGroupStatus next)
    {
        switch (Status)
        {
            case AuditGroupStatus.Idle:
                return next == AuditGroupStatus.Warming || next == AuditGroupStatus.Ready;
            case AuditGroupStatus.Warming:
                switch (next)
                {
                    case AuditGroupStatus.Stopped:
                        return true;
                }
                return false;
            case AuditGroupStatus.Ready:
                return next == AuditGroupStatus.Draining;
            case AuditGroupStatus.Draining:
                return next == AuditGroupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(AuditGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"audit group"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Capacity - Rate, 2);
    }

    public AuditGroup Copy()
    {
        return new AuditGroup(Code, Label, Timezone, Limit, Unit, FloorValue, Rate, Capacity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
