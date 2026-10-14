namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary adjustment assignment is in its life cycle.</summary>
public enum PrimaryAdjustmentAssignmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A primary adjustment assignment as the adjustment team keeps it.</summary>
public sealed class PrimaryAdjustmentAssignment(string code, string title, int retries, double amount, string currency, string checksum, double rate, PrimaryAdjustmentAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Retries { get; set; } = retries;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public string Checksum { get; set; } = checksum;
    public double Rate { get; set; } = rate;
    public PrimaryAdjustmentAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a primary adjustment assignment in status proposed.</summary>
    public static PrimaryAdjustmentAssignment NewPrimaryAdjustmentAssignment(string code, string title)
    {
        return new PrimaryAdjustmentAssignment(
            code,
            title,
            0,
            0.0,
            "",
            "",
            0.0,
            PrimaryAdjustmentAssignmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAdjustmentAssignmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAdjustmentAssignmentStatus next)
    {
        switch (Status)
        {
            case PrimaryAdjustmentAssignmentStatus.Proposed:
                switch (next)
                {
                    case PrimaryAdjustmentAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case PrimaryAdjustmentAssignmentStatus.Approved:
                return next == PrimaryAdjustmentAssignmentStatus.Rejected;
            case PrimaryAdjustmentAssignmentStatus.Rejected:
                switch (next)
                {
                    case PrimaryAdjustmentAssignmentStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAdjustmentAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary adjustment assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary adjustment assignment"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Retries + Rate, 2);
    }

    public PrimaryAdjustmentAssignment Copy()
    {
        return new PrimaryAdjustmentAssignment(Code, Title, Retries, Amount, Currency, Checksum, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
