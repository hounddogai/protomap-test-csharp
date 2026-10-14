namespace Acme.Workforce.Contractors.StatementOfWork.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual statement of work group is in its life cycle.</summary>
public enum ManualStatementOfWorkGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual statement of work group. Its code is unique within a tenant.</summary>
public sealed class ManualStatementOfWorkGroup(string code, string label, double rate, string summary, string costCenter, double amount, ManualStatementOfWorkGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public string Summary { get; set; } = summary;
    public string CostCenter { get; set; } = costCenter;
    public double Amount { get; set; } = amount;
    public ManualStatementOfWorkGroupStatus Status { get; set; } = status;

    /// <summary>Starts a manual statement of work group in status planned.</summary>
    public static ManualStatementOfWorkGroup NewManualStatementOfWorkGroup(string code, string label)
    {
        return new ManualStatementOfWorkGroup(
            code,
            label,
            0.0,
            "",
            "",
            0.0,
            ManualStatementOfWorkGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualStatementOfWorkGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualStatementOfWorkGroupStatus next)
    {
        switch (Status)
        {
            case ManualStatementOfWorkGroupStatus.Planned:
                return next == ManualStatementOfWorkGroupStatus.Completed || next == ManualStatementOfWorkGroupStatus.Cancelled;
            case ManualStatementOfWorkGroupStatus.Scheduled:
                return next == ManualStatementOfWorkGroupStatus.Completed;
            case ManualStatementOfWorkGroupStatus.InProgress:
                return next == ManualStatementOfWorkGroupStatus.Completed || next == ManualStatementOfWorkGroupStatus.Cancelled;
            case ManualStatementOfWorkGroupStatus.Completed:
                switch (next)
                {
                    case ManualStatementOfWorkGroupStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualStatementOfWorkGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual statement of work group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedValue()
    {
        return Math.Round(Rate + Amount, 2);
    }

    public ManualStatementOfWorkGroup Copy()
    {
        return new ManualStatementOfWorkGroup(Code, Label, Rate, Summary, CostCenter, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
