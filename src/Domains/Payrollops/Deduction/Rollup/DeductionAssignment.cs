namespace Acme.Workforce.Payrollops.Deduction.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction assignment is in its life cycle.</summary>
public enum DeductionAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One deduction assignment, with what the planners need to act on it.</summary>
public sealed class DeductionAssignment(string code, string title, int createdAt, string checksum, string channel, int sequence, double maximum, double discount, DeductionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int CreatedAt { get; set; } = createdAt;
    public string Checksum { get; set; } = checksum;
    public string Channel { get; set; } = channel;
    public int Sequence { get; set; } = sequence;
    public double Maximum { get; set; } = maximum;
    public double Discount { get; set; } = discount;
    public DeductionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a deduction assignment in status proposed.</summary>
    public static DeductionAssignment NewDeductionAssignment(string code, string title)
    {
        return new DeductionAssignment(code, title, 0, "", "", 0, 0.0, 0.0, DeductionAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionAssignmentStatus next)
    {
        switch (Status)
        {
            case DeductionAssignmentStatus.Proposed:
                return next == DeductionAssignmentStatus.Approved || next == DeductionAssignmentStatus.Rejected;
            case DeductionAssignmentStatus.Approved:
                return next == DeductionAssignmentStatus.Rejected || next == DeductionAssignmentStatus.Withdrawn;
            case DeductionAssignmentStatus.Rejected:
                return next == DeductionAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DeductionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"deduction assignment"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)CreatedAt * Discount, 2);
    }

    public DeductionAssignment Copy()
    {
        return new DeductionAssignment(
            Code,
            Title,
            CreatedAt,
            Checksum,
            Channel,
            Sequence,
            Maximum,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
