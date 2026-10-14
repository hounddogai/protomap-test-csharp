namespace Acme.Workforce.Contractors.StatementOfWork.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default statement of work audit is in its life cycle.</summary>
public enum DefaultStatementOfWorkAuditStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a default statement of work audit and its current status.</summary>
public sealed class DefaultStatementOfWorkAudit(string code, string name, double discount, double cost, int batchSize, string reviewer, string description, int retries, string region, DefaultStatementOfWorkAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double Cost { get; set; } = cost;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public string Description { get; set; } = description;
    public int Retries { get; set; } = retries;
    public string Region { get; set; } = region;
    public DefaultStatementOfWorkAuditStatus Status { get; set; } = status;

    /// <summary>Starts a default statement of work audit in status new.</summary>
    public static DefaultStatementOfWorkAudit NewDefaultStatementOfWorkAudit(string code, string name)
    {
        return new DefaultStatementOfWorkAudit(
            code,
            name,
            0.0,
            0.0,
            0,
            "",
            "",
            0,
            "",
            DefaultStatementOfWorkAuditStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultStatementOfWorkAuditStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultStatementOfWorkAuditStatus next)
    {
        switch (Status)
        {
            case DefaultStatementOfWorkAuditStatus.New:
                return next == DefaultStatementOfWorkAuditStatus.Running || next == DefaultStatementOfWorkAuditStatus.Done;
            case DefaultStatementOfWorkAuditStatus.Queued:
                return next == DefaultStatementOfWorkAuditStatus.Done || next == DefaultStatementOfWorkAuditStatus.Failed;
            case DefaultStatementOfWorkAuditStatus.Running:
                switch (next)
                {
                    case DefaultStatementOfWorkAuditStatus.Done:
                    case DefaultStatementOfWorkAuditStatus.Failed:
                        return true;
                }
                return false;
            case DefaultStatementOfWorkAuditStatus.Done:
                switch (next)
                {
                    case DefaultStatementOfWorkAuditStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultStatementOfWorkAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default statement of work audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)Retries - (double)BatchSize, 2);
    }

    public DefaultStatementOfWorkAudit Copy()
    {
        return new DefaultStatementOfWorkAudit(
            Code,
            Name,
            Discount,
            Cost,
            BatchSize,
            Reviewer,
            Description,
            Retries,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
