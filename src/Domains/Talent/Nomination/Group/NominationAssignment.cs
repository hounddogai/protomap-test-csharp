namespace Acme.Workforce.Talent.Nomination.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination assignment is in its life cycle.</summary>
public enum NominationAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A nomination assignment as the nomination team keeps it.</summary>
public sealed class NominationAssignment(string code, string label, double hours, string channel, int limit, double score, bool approved, string checksum, NominationAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public string Channel { get; set; } = channel;
    public int Limit { get; set; } = limit;
    public double Score { get; set; } = score;
    public bool Approved { get; set; } = approved;
    public string Checksum { get; set; } = checksum;
    public NominationAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a nomination assignment in status idle.</summary>
    public static NominationAssignment NewNominationAssignment(string code, string label)
    {
        return new NominationAssignment(code, label, 0.0, "", 0, 0.0, false, "", NominationAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationAssignmentStatus next)
    {
        switch (Status)
        {
            case NominationAssignmentStatus.Idle:
                switch (next)
                {
                    case NominationAssignmentStatus.Warming:
                    case NominationAssignmentStatus.Ready:
                        return true;
                }
                return false;
            case NominationAssignmentStatus.Warming:
                switch (next)
                {
                    case NominationAssignmentStatus.Ready:
                    case NominationAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case NominationAssignmentStatus.Ready:
                switch (next)
                {
                    case NominationAssignmentStatus.Draining:
                    case NominationAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case NominationAssignmentStatus.Draining:
                switch (next)
                {
                    case NominationAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(NominationAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"nomination assignment"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Hours - Score, 2);
    }

    public NominationAssignment Copy()
    {
        return new NominationAssignment(Code, Label, Hours, Channel, Limit, Score, Approved, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
