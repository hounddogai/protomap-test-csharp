namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor assignment is in its life cycle.</summary>
public enum VisitorAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a visitor assignment and its current status.</summary>
public sealed class VisitorAssignment(string code, string title, int expiresAt, int windowDays, double weight, double amount, string category, string notes, string externalRef, VisitorAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int WindowDays { get; set; } = windowDays;
    public double Weight { get; set; } = weight;
    public double Amount { get; set; } = amount;
    public string Category { get; set; } = category;
    public string Notes { get; set; } = notes;
    public string ExternalRef { get; set; } = externalRef;
    public VisitorAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a visitor assignment in status idle.</summary>
    public static VisitorAssignment NewVisitorAssignment(string code, string title)
    {
        return new VisitorAssignment(code, title, 0, 0, 0.0, 0.0, "", "", "", VisitorAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorAssignmentStatus next)
    {
        switch (Status)
        {
            case VisitorAssignmentStatus.Idle:
                switch (next)
                {
                    case VisitorAssignmentStatus.Draining:
                    case VisitorAssignmentStatus.Stopped:
                        return true;
                }
                return false;
            case VisitorAssignmentStatus.Warming:
                return next == VisitorAssignmentStatus.Stopped;
            case VisitorAssignmentStatus.Ready:
                return next == VisitorAssignmentStatus.Stopped;
            case VisitorAssignmentStatus.Draining:
                switch (next)
                {
                    case VisitorAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(VisitorAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)WindowDays * Amount, 2);
    }

    public VisitorAssignment Copy()
    {
        return new VisitorAssignment(
            Code,
            Title,
            ExpiresAt,
            WindowDays,
            Weight,
            Amount,
            Category,
            Notes,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
