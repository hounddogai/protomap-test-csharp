namespace Acme.Workforce.Workplace.AccessBadge.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback access badge assignment is in its life cycle.</summary>
public enum FallbackAccessBadgeAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a fallback access badge assignment and its current status.</summary>
public sealed class FallbackAccessBadgeAssignment(string code, string name, int attempts, string region, string grade, FallbackAccessBadgeAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string Region { get; set; } = region;
    public string Grade { get; set; } = grade;
    public FallbackAccessBadgeAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback access badge assignment in status idle.</summary>
    public static FallbackAccessBadgeAssignment NewFallbackAccessBadgeAssignment(string code, string name)
    {
        return new FallbackAccessBadgeAssignment(code, name, 0, "", "", FallbackAccessBadgeAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAccessBadgeAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAccessBadgeAssignmentStatus next)
    {
        switch (Status)
        {
            case FallbackAccessBadgeAssignmentStatus.Idle:
                switch (next)
                {
                    case FallbackAccessBadgeAssignmentStatus.Ready:
                        return true;
                }
                return false;
            case FallbackAccessBadgeAssignmentStatus.Warming:
                switch (next)
                {
                    case FallbackAccessBadgeAssignmentStatus.Ready:
                    case FallbackAccessBadgeAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case FallbackAccessBadgeAssignmentStatus.Ready:
                return next == FallbackAccessBadgeAssignmentStatus.Stopped;
            case FallbackAccessBadgeAssignmentStatus.Draining:
                switch (next)
                {
                    case FallbackAccessBadgeAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackAccessBadgeAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback access badge assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Attempts;
    }

    public FallbackAccessBadgeAssignment Copy()
    {
        return new FallbackAccessBadgeAssignment(Code, Name, Attempts, Region, Grade, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
