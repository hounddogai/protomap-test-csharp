namespace Acme.Workforce.Payrollops.RetroPay.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay assignment is in its life cycle.</summary>
public enum RetroPayAssignmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A retro pay assignment. Its code is unique within a tenant.</summary>
public sealed class RetroPayAssignment(string code, string label, string ownerId, string reviewer, string externalRef, int quantity, RetroPayAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public string Reviewer { get; set; } = reviewer;
    public string ExternalRef { get; set; } = externalRef;
    public int Quantity { get; set; } = quantity;
    public RetroPayAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay assignment in status open.</summary>
    public static RetroPayAssignment NewRetroPayAssignment(string code, string label)
    {
        return new RetroPayAssignment(code, label, "", "", "", 0, RetroPayAssignmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayAssignmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayAssignmentStatus next)
    {
        switch (Status)
        {
            case RetroPayAssignmentStatus.Open:
                return next == RetroPayAssignmentStatus.Resolved;
            case RetroPayAssignmentStatus.InReview:
                switch (next)
                {
                    case RetroPayAssignmentStatus.Resolved:
                        return true;
                }
                return false;
            case RetroPayAssignmentStatus.Resolved:
                switch (next)
                {
                    case RetroPayAssignmentStatus.Archived:
                        return true;
                }
                return false;
            case RetroPayAssignmentStatus.Reopened:
                return next == RetroPayAssignmentStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RetroPayAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"retro pay assignment"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public RetroPayAssignment Copy()
    {
        return new RetroPayAssignment(Code, Label, OwnerId, Reviewer, ExternalRef, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
