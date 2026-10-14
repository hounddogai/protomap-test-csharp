namespace Acme.Workforce.Absence.Accrual.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual assignment is in its life cycle.</summary>
public enum AccrualAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One accrual assignment, with what the planners need to act on it.</summary>
public sealed class AccrualAssignment(string code, string title, string grade, int endsAt, List<string> tags, string checksum, int limit, AccrualAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public int EndsAt { get; set; } = endsAt;
    public List<string> Tags { get; set; } = tags;
    public string Checksum { get; set; } = checksum;
    public int Limit { get; set; } = limit;
    public AccrualAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a accrual assignment in status proposed.</summary>
    public static AccrualAssignment NewAccrualAssignment(string code, string title)
    {
        return new AccrualAssignment(
            code,
            title,
            "",
            0,
            new List<string>(),
            "",
            0,
            AccrualAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualAssignmentStatus next)
    {
        switch (Status)
        {
            case AccrualAssignmentStatus.Proposed:
                switch (next)
                {
                    case AccrualAssignmentStatus.Approved:
                    case AccrualAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case AccrualAssignmentStatus.Approved:
                return next == AccrualAssignmentStatus.Rejected;
            case AccrualAssignmentStatus.Rejected:
                return next == AccrualAssignmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AccrualAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)EndsAt + (double)Limit, 2);
    }

    public AccrualAssignment Copy()
    {
        return new AccrualAssignment(Code, Title, Grade, EndsAt, Tags, Checksum, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
