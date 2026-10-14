namespace Acme.Workforce.Compliance.PolicyAck.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered policy ack schedule is in its life cycle.</summary>
public enum TieredPolicyAckScheduleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a tiered policy ack schedule and its current status.</summary>
public sealed class TieredPolicyAckSchedule(string code, string title, double maximum, string source, bool pinned, int sequence, TieredPolicyAckScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public string Source { get; set; } = source;
    public bool Pinned { get; set; } = pinned;
    public int Sequence { get; set; } = sequence;
    public TieredPolicyAckScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a tiered policy ack schedule in status proposed.</summary>
    public static TieredPolicyAckSchedule NewTieredPolicyAckSchedule(string code, string title)
    {
        return new TieredPolicyAckSchedule(code, title, 0.0, "", false, 0, TieredPolicyAckScheduleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPolicyAckScheduleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPolicyAckScheduleStatus next)
    {
        switch (Status)
        {
            case TieredPolicyAckScheduleStatus.Proposed:
                switch (next)
                {
                    case TieredPolicyAckScheduleStatus.Approved:
                    case TieredPolicyAckScheduleStatus.Withdrawn:
                        return true;
                }
                return false;
            case TieredPolicyAckScheduleStatus.Approved:
                return next == TieredPolicyAckScheduleStatus.Rejected || next == TieredPolicyAckScheduleStatus.Withdrawn;
            case TieredPolicyAckScheduleStatus.Rejected:
                return next == TieredPolicyAckScheduleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(TieredPolicyAckScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered policy ack schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Sequence * Maximum, 2);
    }

    public TieredPolicyAckSchedule Copy()
    {
        return new TieredPolicyAckSchedule(Code, Title, Maximum, Source, Pinned, Sequence, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
