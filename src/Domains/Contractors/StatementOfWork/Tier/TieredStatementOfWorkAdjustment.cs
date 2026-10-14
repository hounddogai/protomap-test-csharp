namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered statement of work adjustment is in its life cycle.</summary>
public enum TieredStatementOfWorkAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A tiered statement of work adjustment as the statement of work team keeps it.</summary>
public sealed class TieredStatementOfWorkAdjustment(string code, string label, string locale, int leaveDays, double discount, string externalRef, double minimum, string country, double amount, int attempts, string description, TieredStatementOfWorkAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public int LeaveDays { get; set; } = leaveDays;
    public double Discount { get; set; } = discount;
    public string ExternalRef { get; set; } = externalRef;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public double Amount { get; set; } = amount;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public TieredStatementOfWorkAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a tiered statement of work adjustment in status proposed.</summary>
    public static TieredStatementOfWorkAdjustment NewTieredStatementOfWorkAdjustment(string code, string label)
    {
        return new TieredStatementOfWorkAdjustment(
            code,
            label,
            "",
            0,
            0.0,
            "",
            0.0,
            "",
            0.0,
            0,
            "",
            TieredStatementOfWorkAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredStatementOfWorkAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredStatementOfWorkAdjustmentStatus next)
    {
        switch (Status)
        {
            case TieredStatementOfWorkAdjustmentStatus.Proposed:
                switch (next)
                {
                    case TieredStatementOfWorkAdjustmentStatus.Approved:
                    case TieredStatementOfWorkAdjustmentStatus.Rejected:
                        return true;
                }
                return false;
            case TieredStatementOfWorkAdjustmentStatus.Approved:
                switch (next)
                {
                    case TieredStatementOfWorkAdjustmentStatus.Rejected:
                    case TieredStatementOfWorkAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case TieredStatementOfWorkAdjustmentStatus.Rejected:
                switch (next)
                {
                    case TieredStatementOfWorkAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredStatementOfWorkAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered statement of work adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Attempts + Amount, 2);
    }

    public TieredStatementOfWorkAdjustment Copy()
    {
        return new TieredStatementOfWorkAdjustment(
            Code,
            Label,
            Locale,
            LeaveDays,
            Discount,
            ExternalRef,
            Minimum,
            Country,
            Amount,
            Attempts,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
