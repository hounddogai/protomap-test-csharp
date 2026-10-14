namespace Acme.Workforce.Compliance.Audit.AuditRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict audit record is in its life cycle.</summary>
public enum StrictAuditRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One strict audit record, with what the planners need to act on it.</summary>
public sealed class StrictAuditRecord(string code, string title, string salaryBand, int capacity, double weight, double hours, double cost, bool active, StrictAuditRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public int Capacity { get; set; } = capacity;
    public double Weight { get; set; } = weight;
    public double Hours { get; set; } = hours;
    public double Cost { get; set; } = cost;
    public bool Active { get; set; } = active;
    public StrictAuditRecordStatus Status { get; set; } = status;

    /// <summary>Starts a strict audit record in status open.</summary>
    public static StrictAuditRecord NewStrictAuditRecord(string code, string title)
    {
        return new StrictAuditRecord(code, title, "", 0, 0.0, 0.0, 0.0, false, StrictAuditRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictAuditRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictAuditRecordStatus next)
    {
        switch (Status)
        {
            case StrictAuditRecordStatus.Open:
                return next == StrictAuditRecordStatus.InReview || next == StrictAuditRecordStatus.Reopened;
            case StrictAuditRecordStatus.InReview:
                return next == StrictAuditRecordStatus.Resolved;
            case StrictAuditRecordStatus.Resolved:
                return next == StrictAuditRecordStatus.Reopened || next == StrictAuditRecordStatus.Archived;
            case StrictAuditRecordStatus.Reopened:
                return next == StrictAuditRecordStatus.Archived;
        }
        return false;
    }

    public void MoveTo(StrictAuditRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict audit record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Cost - Weight, 2);
    }

    public StrictAuditRecord Copy()
    {
        return new StrictAuditRecord(Code, Title, SalaryBand, Capacity, Weight, Hours, Cost, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
