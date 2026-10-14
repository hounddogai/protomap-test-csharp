namespace Acme.Workforce.Contractors.StatementOfWork.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive statement of work audit is in its life cycle.</summary>
public enum AdaptiveStatementOfWorkAuditStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A adaptive statement of work audit as the statement of work team keeps it.</summary>
public sealed class AdaptiveStatementOfWorkAudit(string code, string label, string channel, double hours, double amount, int quantity, string name, AdaptiveStatementOfWorkAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public double Hours { get; set; } = hours;
    public double Amount { get; set; } = amount;
    public int Quantity { get; set; } = quantity;
    public string Name { get; set; } = name;
    public AdaptiveStatementOfWorkAuditStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive statement of work audit in status proposed.</summary>
    public static AdaptiveStatementOfWorkAudit NewAdaptiveStatementOfWorkAudit(string code, string label)
    {
        return new AdaptiveStatementOfWorkAudit(
            code,
            label,
            "",
            0.0,
            0.0,
            0,
            "",
            AdaptiveStatementOfWorkAuditStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveStatementOfWorkAuditStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveStatementOfWorkAuditStatus next)
    {
        switch (Status)
        {
            case AdaptiveStatementOfWorkAuditStatus.Proposed:
                return next == AdaptiveStatementOfWorkAuditStatus.Approved || next == AdaptiveStatementOfWorkAuditStatus.Withdrawn;
            case AdaptiveStatementOfWorkAuditStatus.Approved:
                return next == AdaptiveStatementOfWorkAuditStatus.Rejected;
            case AdaptiveStatementOfWorkAuditStatus.Rejected:
                return next == AdaptiveStatementOfWorkAuditStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AdaptiveStatementOfWorkAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive statement of work audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Hours + (double)Quantity, 2);
    }

    public AdaptiveStatementOfWorkAudit Copy()
    {
        return new AdaptiveStatementOfWorkAudit(Code, Label, Channel, Hours, Amount, Quantity, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
