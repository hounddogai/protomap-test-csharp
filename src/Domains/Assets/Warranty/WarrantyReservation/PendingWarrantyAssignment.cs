namespace Acme.Workforce.Assets.Warranty.WarrantyReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending warranty assignment is in its life cycle.</summary>
public enum PendingWarrantyAssignmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a pending warranty assignment and its current status.</summary>
public sealed class PendingWarrantyAssignment(string code, string title, string grade, double amount, bool approved, PendingWarrantyAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public bool Approved { get; set; } = approved;
    public PendingWarrantyAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a pending warranty assignment in status created.</summary>
    public static PendingWarrantyAssignment NewPendingWarrantyAssignment(string code, string title)
    {
        return new PendingWarrantyAssignment(code, title, "", 0.0, false, PendingWarrantyAssignmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingWarrantyAssignmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingWarrantyAssignmentStatus next)
    {
        switch (Status)
        {
            case PendingWarrantyAssignmentStatus.Created:
                return next == PendingWarrantyAssignmentStatus.Published;
            case PendingWarrantyAssignmentStatus.Validated:
                switch (next)
                {
                    case PendingWarrantyAssignmentStatus.Published:
                    case PendingWarrantyAssignmentStatus.Retired:
                        return true;
                }
                return false;
            case PendingWarrantyAssignmentStatus.Published:
                return next == PendingWarrantyAssignmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PendingWarrantyAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending warranty assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public PendingWarrantyAssignment Copy()
    {
        return new PendingWarrantyAssignment(Code, Title, Grade, Amount, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
