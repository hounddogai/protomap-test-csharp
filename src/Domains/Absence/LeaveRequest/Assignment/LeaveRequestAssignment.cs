namespace Acme.Workforce.Absence.LeaveRequest.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a leave request assignment is in its life cycle.</summary>
public enum LeaveRequestAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A leave request assignment. Its code is unique within a tenant.</summary>
public sealed class LeaveRequestAssignment(string code, string title, int endsAt, Dictionary<string, string> attributes, int revision, int leaveDays, double rating, LeaveRequestAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Revision { get; set; } = revision;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rating { get; set; } = rating;
    public LeaveRequestAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a leave request assignment in status planned.</summary>
    public static LeaveRequestAssignment NewLeaveRequestAssignment(string code, string title)
    {
        return new LeaveRequestAssignment(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            0,
            0,
            0.0,
            LeaveRequestAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LeaveRequestAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LeaveRequestAssignmentStatus next)
    {
        switch (Status)
        {
            case LeaveRequestAssignmentStatus.Planned:
                switch (next)
                {
                    case LeaveRequestAssignmentStatus.Scheduled:
                        return true;
                }
                return false;
            case LeaveRequestAssignmentStatus.Scheduled:
                switch (next)
                {
                    case LeaveRequestAssignmentStatus.InProgress:
                        return true;
                }
                return false;
            case LeaveRequestAssignmentStatus.InProgress:
                return next == LeaveRequestAssignmentStatus.Cancelled;
            case LeaveRequestAssignmentStatus.Completed:
                return next == LeaveRequestAssignmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LeaveRequestAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"leave request assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round((double)EndsAt + Rating, 2);
    }

    public LeaveRequestAssignment Copy()
    {
        return new LeaveRequestAssignment(Code, Title, EndsAt, Attributes, Revision, LeaveDays, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
