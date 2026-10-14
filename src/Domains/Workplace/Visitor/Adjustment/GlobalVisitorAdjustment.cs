namespace Acme.Workforce.Workplace.Visitor.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global visitor adjustment is in its life cycle.</summary>
public enum GlobalVisitorAdjustmentStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a global visitor adjustment and its current status.</summary>
public sealed class GlobalVisitorAdjustment(string code, string label, double rate, string ownerId, string managerId, int retries, string summary, string grade, int quantity, GlobalVisitorAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string OwnerId { get; set; } = ownerId;
    public string ManagerId { get; set; } = managerId;
    public int Retries { get; set; } = retries;
    public string Summary { get; set; } = summary;
    public string Grade { get; set; } = grade;
    public int Quantity { get; set; } = quantity;
    public GlobalVisitorAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a global visitor adjustment in status created.</summary>
    public static GlobalVisitorAdjustment NewGlobalVisitorAdjustment(string code, string label)
    {
        return new GlobalVisitorAdjustment(
            code,
            label,
            0.0,
            "",
            "",
            0,
            "",
            "",
            0,
            GlobalVisitorAdjustmentStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalVisitorAdjustmentStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalVisitorAdjustmentStatus next)
    {
        switch (Status)
        {
            case GlobalVisitorAdjustmentStatus.Created:
                return next == GlobalVisitorAdjustmentStatus.Validated || next == GlobalVisitorAdjustmentStatus.Retired;
            case GlobalVisitorAdjustmentStatus.Validated:
                return next == GlobalVisitorAdjustmentStatus.Retired;
            case GlobalVisitorAdjustmentStatus.Published:
                return next == GlobalVisitorAdjustmentStatus.Retired;
        }
        return false;
    }

    public void MoveTo(GlobalVisitorAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global visitor adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)Retries + (double)Quantity, 2);
    }

    public GlobalVisitorAdjustment Copy()
    {
        return new GlobalVisitorAdjustment(
            Code,
            Label,
            Rate,
            OwnerId,
            ManagerId,
            Retries,
            Summary,
            Grade,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
