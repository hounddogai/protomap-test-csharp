namespace Acme.Workforce.Assets.LicenseSeat.StrictSeatReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending license seat slot is in its life cycle.</summary>
public enum PendingLicenseSeatSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pending license seat slot and its current status.</summary>
public sealed class PendingLicenseSeatSlot(string code, string label, string category, bool active, int priority, double cost, PendingLicenseSeatSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public int Priority { get; set; } = priority;
    public double Cost { get; set; } = cost;
    public PendingLicenseSeatSlotStatus Status { get; set; } = status;

    /// <summary>Starts a pending license seat slot in status open.</summary>
    public static PendingLicenseSeatSlot NewPendingLicenseSeatSlot(string code, string label)
    {
        return new PendingLicenseSeatSlot(code, label, "", false, 0, 0.0, PendingLicenseSeatSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingLicenseSeatSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingLicenseSeatSlotStatus next)
    {
        switch (Status)
        {
            case PendingLicenseSeatSlotStatus.Open:
                switch (next)
                {
                    case PendingLicenseSeatSlotStatus.InReview:
                        return true;
                }
                return false;
            case PendingLicenseSeatSlotStatus.InReview:
                return next == PendingLicenseSeatSlotStatus.Archived;
            case PendingLicenseSeatSlotStatus.Resolved:
                switch (next)
                {
                    case PendingLicenseSeatSlotStatus.Reopened:
                    case PendingLicenseSeatSlotStatus.Archived:
                        return true;
                }
                return false;
            case PendingLicenseSeatSlotStatus.Reopened:
                return next == PendingLicenseSeatSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PendingLicenseSeatSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending license seat slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedValue()
    {
        return Math.Round(Cost - (double)Priority, 2);
    }

    public PendingLicenseSeatSlot Copy()
    {
        return new PendingLicenseSeatSlot(Code, Label, Category, Active, Priority, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
