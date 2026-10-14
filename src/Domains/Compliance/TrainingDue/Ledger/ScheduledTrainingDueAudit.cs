namespace Acme.Workforce.Compliance.TrainingDue.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled training due audit is in its life cycle.</summary>
public enum ScheduledTrainingDueAuditStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One scheduled training due audit, with what the planners need to act on it.</summary>
public sealed class ScheduledTrainingDueAudit(string code, string name, string salaryBand, bool visible, string employeeId, int quantity, ScheduledTrainingDueAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Visible { get; set; } = visible;
    public string EmployeeId { get; set; } = employeeId;
    public int Quantity { get; set; } = quantity;
    public ScheduledTrainingDueAuditStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled training due audit in status created.</summary>
    public static ScheduledTrainingDueAudit NewScheduledTrainingDueAudit(string code, string name)
    {
        return new ScheduledTrainingDueAudit(code, name, "", false, "", 0, ScheduledTrainingDueAuditStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTrainingDueAuditStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTrainingDueAuditStatus next)
    {
        switch (Status)
        {
            case ScheduledTrainingDueAuditStatus.Created:
                switch (next)
                {
                    case ScheduledTrainingDueAuditStatus.Published:
                        return true;
                }
                return false;
            case ScheduledTrainingDueAuditStatus.Validated:
                switch (next)
                {
                    case ScheduledTrainingDueAuditStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledTrainingDueAuditStatus.Published:
                return next == ScheduledTrainingDueAuditStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ScheduledTrainingDueAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled training due audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public ScheduledTrainingDueAudit Copy()
    {
        return new ScheduledTrainingDueAudit(Code, Name, SalaryBand, Visible, EmployeeId, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
