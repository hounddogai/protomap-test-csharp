namespace Acme.Workforce.Absence.LeaveRequest.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request record is in its life cycle.</summary>
public enum LeaveRequestRecordStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One leave request record, with what the planners need to act on it.</summary>
public sealed class LeaveRequestRecord(string code, string label, string grade, string managerId, string unit, int sequence, string segment, LeaveRequestRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public string Unit { get; set; } = unit;
    public int Sequence { get; set; } = sequence;
    public string Segment { get; set; } = segment;
    public LeaveRequestRecordStatus Status { get; set; } = status;

    /// <summary>Starts a leave request record in status idle.</summary>
    public static LeaveRequestRecord NewLeaveRequestRecord(string code, string label)
    {
        return new LeaveRequestRecord(code, label, "", "", "", 0, "", LeaveRequestRecordStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestRecordStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestRecordStatus next)
    {
        switch (Status)
        {
            case LeaveRequestRecordStatus.Idle:
                return next == LeaveRequestRecordStatus.Warming || next == LeaveRequestRecordStatus.Draining;
            case LeaveRequestRecordStatus.Warming:
                switch (next)
                {
                    case LeaveRequestRecordStatus.Ready:
                    case LeaveRequestRecordStatus.Stopped:
                        return true;
                }
                return false;
            case LeaveRequestRecordStatus.Ready:
                switch (next)
                {
                    case LeaveRequestRecordStatus.Draining:
                    case LeaveRequestRecordStatus.Stopped:
                        return true;
                }
                return false;
            case LeaveRequestRecordStatus.Draining:
                switch (next)
                {
                    case LeaveRequestRecordStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LeaveRequestRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)Sequence;
    }

    public LeaveRequestRecord Copy()
    {
        return new LeaveRequestRecord(Code, Label, Grade, ManagerId, Unit, Sequence, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
