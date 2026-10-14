namespace Acme.Workforce.Assets.Warranty.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty entry is in its life cycle.</summary>
public enum WarrantyEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A warranty entry as the warranty team keeps it.</summary>
public sealed class WarrantyEntry(string code, string label, bool archived, string reviewer, string source, string employeeId, double amount, double ceiling, double discount, string phone, double fte, WarrantyEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Archived { get; set; } = archived;
    public string Reviewer { get; set; } = reviewer;
    public string Source { get; set; } = source;
    public string EmployeeId { get; set; } = employeeId;
    public double Amount { get; set; } = amount;
    public double Ceiling { get; set; } = ceiling;
    public double Discount { get; set; } = discount;
    public string Phone { get; set; } = phone;
    public double Fte { get; set; } = fte;
    public WarrantyEntryStatus Status { get; set; } = status;

    /// <summary>Starts a warranty entry in status planned.</summary>
    public static WarrantyEntry NewWarrantyEntry(string code, string label)
    {
        return new WarrantyEntry(
            code,
            label,
            false,
            "",
            "",
            "",
            0.0,
            0.0,
            0.0,
            "",
            0.0,
            WarrantyEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyEntryStatus next)
    {
        switch (Status)
        {
            case WarrantyEntryStatus.Planned:
                switch (next)
                {
                    case WarrantyEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case WarrantyEntryStatus.Scheduled:
                return next == WarrantyEntryStatus.Completed || next == WarrantyEntryStatus.Cancelled;
            case WarrantyEntryStatus.InProgress:
                switch (next)
                {
                    case WarrantyEntryStatus.Completed:
                        return true;
                }
                return false;
            case WarrantyEntryStatus.Completed:
                switch (next)
                {
                    case WarrantyEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Amount + Discount, 2);
    }

    public WarrantyEntry Copy()
    {
        return new WarrantyEntry(
            Code,
            Label,
            Archived,
            Reviewer,
            Source,
            EmployeeId,
            Amount,
            Ceiling,
            Discount,
            Phone,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
