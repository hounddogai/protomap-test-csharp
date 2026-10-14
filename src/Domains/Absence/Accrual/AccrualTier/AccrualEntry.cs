namespace Acme.Workforce.Absence.Accrual.AccrualTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual entry is in its life cycle.</summary>
public enum AccrualEntryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A accrual entry. Its code is unique within a tenant.</summary>
public sealed class AccrualEntry(string code, string label, string employeeId, double score, double discount, AccrualEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public double Discount { get; set; } = discount;
    public AccrualEntryStatus Status { get; set; } = status;

    /// <summary>Starts a accrual entry in status proposed.</summary>
    public static AccrualEntry NewAccrualEntry(string code, string label)
    {
        return new AccrualEntry(code, label, "", 0.0, 0.0, AccrualEntryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualEntryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualEntryStatus next)
    {
        switch (Status)
        {
            case AccrualEntryStatus.Proposed:
                switch (next)
                {
                    case AccrualEntryStatus.Approved:
                    case AccrualEntryStatus.Rejected:
                        return true;
                }
                return false;
            case AccrualEntryStatus.Approved:
                return next == AccrualEntryStatus.Withdrawn;
            case AccrualEntryStatus.Rejected:
                switch (next)
                {
                    case AccrualEntryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedValue()
    {
        return Math.Round(Discount + Score, 2);
    }

    public AccrualEntry Copy()
    {
        return new AccrualEntry(Code, Label, EmployeeId, Score, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
