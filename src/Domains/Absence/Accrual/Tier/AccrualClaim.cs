namespace Acme.Workforce.Absence.Accrual.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual claim is in its life cycle.</summary>
public enum AccrualClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a accrual claim and its current status.</summary>
public sealed class AccrualClaim(string code, string label, int version, string department, string employeeId, int quantity, string badge, double margin, double discount, int retries, AccrualClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Version { get; set; } = version;
    public string Department { get; set; } = department;
    public string EmployeeId { get; set; } = employeeId;
    public int Quantity { get; set; } = quantity;
    public string Badge { get; set; } = badge;
    public double Margin { get; set; } = margin;
    public double Discount { get; set; } = discount;
    public int Retries { get; set; } = retries;
    public AccrualClaimStatus Status { get; set; } = status;

    /// <summary>Starts a accrual claim in status planned.</summary>
    public static AccrualClaim NewAccrualClaim(string code, string label)
    {
        return new AccrualClaim(code, label, 0, "", "", 0, "", 0.0, 0.0, 0, AccrualClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualClaimStatus next)
    {
        switch (Status)
        {
            case AccrualClaimStatus.Planned:
                return next == AccrualClaimStatus.Completed;
            case AccrualClaimStatus.Scheduled:
                switch (next)
                {
                    case AccrualClaimStatus.InProgress:
                    case AccrualClaimStatus.Completed:
                        return true;
                }
                return false;
            case AccrualClaimStatus.InProgress:
                switch (next)
                {
                    case AccrualClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case AccrualClaimStatus.Completed:
                switch (next)
                {
                    case AccrualClaimStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Margin * (double)Version, 2);
    }

    public AccrualClaim Copy()
    {
        return new AccrualClaim(
            Code,
            Label,
            Version,
            Department,
            EmployeeId,
            Quantity,
            Badge,
            Margin,
            Discount,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
