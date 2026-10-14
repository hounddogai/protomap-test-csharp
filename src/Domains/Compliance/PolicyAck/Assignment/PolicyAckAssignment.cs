namespace Acme.Workforce.Compliance.PolicyAck.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack assignment is in its life cycle.</summary>
public enum PolicyAckAssignmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One policy ack assignment, with what the planners need to act on it.</summary>
public sealed class PolicyAckAssignment(string code, string title, int attempts, double minimum, int durationMinutes, double ratio, bool pinned, int graceDays, string locale, PolicyAckAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Attempts { get; set; } = attempts;
    public double Minimum { get; set; } = minimum;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Ratio { get; set; } = ratio;
    public bool Pinned { get; set; } = pinned;
    public int GraceDays { get; set; } = graceDays;
    public string Locale { get; set; } = locale;
    public PolicyAckAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack assignment in status planned.</summary>
    public static PolicyAckAssignment NewPolicyAckAssignment(string code, string title)
    {
        return new PolicyAckAssignment(code, title, 0, 0.0, 0, 0.0, false, 0, "", PolicyAckAssignmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckAssignmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckAssignmentStatus next)
    {
        switch (Status)
        {
            case PolicyAckAssignmentStatus.Planned:
                switch (next)
                {
                    case PolicyAckAssignmentStatus.Completed:
                        return true;
                }
                return false;
            case PolicyAckAssignmentStatus.Scheduled:
                switch (next)
                {
                    case PolicyAckAssignmentStatus.Completed:
                    case PolicyAckAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckAssignmentStatus.InProgress:
                switch (next)
                {
                    case PolicyAckAssignmentStatus.Completed:
                    case PolicyAckAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckAssignmentStatus.Completed:
                switch (next)
                {
                    case PolicyAckAssignmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)DurationMinutes + Minimum, 2);
    }

    public PolicyAckAssignment Copy()
    {
        return new PolicyAckAssignment(
            Code,
            Title,
            Attempts,
            Minimum,
            DurationMinutes,
            Ratio,
            Pinned,
            GraceDays,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
