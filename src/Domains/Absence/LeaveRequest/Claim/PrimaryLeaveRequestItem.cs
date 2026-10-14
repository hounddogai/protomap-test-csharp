namespace Acme.Workforce.Absence.LeaveRequest.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary leave request item is in its life cycle.</summary>
public enum PrimaryLeaveRequestItemStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One primary leave request item, with what the planners need to act on it.</summary>
public sealed class PrimaryLeaveRequestItem(string code, string name, int retries, double amount, double discount, int attempts, bool active, double weight, string externalRef, string description, double cost, PrimaryLeaveRequestItemStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public double Discount { get; set; } = discount;
    public int Attempts { get; set; } = attempts;
    public bool Active { get; set; } = active;
    public double Weight { get; set; } = weight;
    public string ExternalRef { get; set; } = externalRef;
    public string Description { get; set; } = description;
    public double Cost { get; set; } = cost;
    public PrimaryLeaveRequestItemStatus Status { get; set; } = status;

    /// <summary>Starts a primary leave request item in status open.</summary>
    public static PrimaryLeaveRequestItem NewPrimaryLeaveRequestItem(string code, string name)
    {
        return new PrimaryLeaveRequestItem(
            code,
            name,
            0,
            0.0,
            0.0,
            0,
            false,
            0.0,
            "",
            "",
            0.0,
            PrimaryLeaveRequestItemStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLeaveRequestItemStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLeaveRequestItemStatus next)
    {
        switch (Status)
        {
            case PrimaryLeaveRequestItemStatus.Open:
                return next == PrimaryLeaveRequestItemStatus.InReview;
            case PrimaryLeaveRequestItemStatus.InReview:
                return next == PrimaryLeaveRequestItemStatus.Resolved;
            case PrimaryLeaveRequestItemStatus.Resolved:
                return next == PrimaryLeaveRequestItemStatus.Reopened || next == PrimaryLeaveRequestItemStatus.Archived;
            case PrimaryLeaveRequestItemStatus.Reopened:
                return next == PrimaryLeaveRequestItemStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryLeaveRequestItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary leave request item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetValue()
    {
        return Math.Round(Amount - Weight, 2);
    }

    public PrimaryLeaveRequestItem Copy()
    {
        return new PrimaryLeaveRequestItem(
            Code,
            Name,
            Retries,
            Amount,
            Discount,
            Attempts,
            Active,
            Weight,
            ExternalRef,
            Description,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
