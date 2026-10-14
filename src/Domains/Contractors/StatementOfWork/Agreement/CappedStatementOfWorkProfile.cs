namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped statement of work profile is in its life cycle.</summary>
public enum CappedStatementOfWorkProfileStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a capped statement of work profile and its current status.</summary>
public sealed class CappedStatementOfWorkProfile(string code, string title, int expiresAt, string externalRef, int capacity, string department, string managerId, string summary, CappedStatementOfWorkProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public string ExternalRef { get; set; } = externalRef;
    public int Capacity { get; set; } = capacity;
    public string Department { get; set; } = department;
    public string ManagerId { get; set; } = managerId;
    public string Summary { get; set; } = summary;
    public CappedStatementOfWorkProfileStatus Status { get; set; } = status;

    /// <summary>Starts a capped statement of work profile in status new.</summary>
    public static CappedStatementOfWorkProfile NewCappedStatementOfWorkProfile(string code, string title)
    {
        return new CappedStatementOfWorkProfile(
            code,
            title,
            0,
            "",
            0,
            "",
            "",
            "",
            CappedStatementOfWorkProfileStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedStatementOfWorkProfileStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedStatementOfWorkProfileStatus next)
    {
        switch (Status)
        {
            case CappedStatementOfWorkProfileStatus.New:
                return next == CappedStatementOfWorkProfileStatus.Running || next == CappedStatementOfWorkProfileStatus.Done;
            case CappedStatementOfWorkProfileStatus.Queued:
                return next == CappedStatementOfWorkProfileStatus.Running;
            case CappedStatementOfWorkProfileStatus.Running:
                switch (next)
                {
                    case CappedStatementOfWorkProfileStatus.Done:
                    case CappedStatementOfWorkProfileStatus.Failed:
                        return true;
                }
                return false;
            case CappedStatementOfWorkProfileStatus.Done:
                return next == CappedStatementOfWorkProfileStatus.Failed;
        }
        return false;
    }

    public void MoveTo(CappedStatementOfWorkProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped statement of work profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Capacity + (double)ExpiresAt, 2);
    }

    public CappedStatementOfWorkProfile Copy()
    {
        return new CappedStatementOfWorkProfile(
            Code,
            Title,
            ExpiresAt,
            ExternalRef,
            Capacity,
            Department,
            ManagerId,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
