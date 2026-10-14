namespace Acme.Workforce.Assets.Laptop.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary laptop slot is in its life cycle.</summary>
public enum PrimaryLaptopSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a primary laptop slot and its current status.</summary>
public sealed class PrimaryLaptopSlot(string code, string title, double discount, double ceiling, string department, PrimaryLaptopSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Discount { get; set; } = discount;
    public double Ceiling { get; set; } = ceiling;
    public string Department { get; set; } = department;
    public PrimaryLaptopSlotStatus Status { get; set; } = status;

    /// <summary>Starts a primary laptop slot in status open.</summary>
    public static PrimaryLaptopSlot NewPrimaryLaptopSlot(string code, string title)
    {
        return new PrimaryLaptopSlot(code, title, 0.0, 0.0, "", PrimaryLaptopSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryLaptopSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryLaptopSlotStatus next)
    {
        switch (Status)
        {
            case PrimaryLaptopSlotStatus.Open:
                return next == PrimaryLaptopSlotStatus.Resolved;
            case PrimaryLaptopSlotStatus.InReview:
                switch (next)
                {
                    case PrimaryLaptopSlotStatus.Resolved:
                        return true;
                }
                return false;
            case PrimaryLaptopSlotStatus.Resolved:
                switch (next)
                {
                    case PrimaryLaptopSlotStatus.Reopened:
                    case PrimaryLaptopSlotStatus.Archived:
                        return true;
                }
                return false;
            case PrimaryLaptopSlotStatus.Reopened:
                return next == PrimaryLaptopSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PrimaryLaptopSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary laptop slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Ceiling + Discount, 2);
    }

    public PrimaryLaptopSlot Copy()
    {
        return new PrimaryLaptopSlot(Code, Title, Discount, Ceiling, Department, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
