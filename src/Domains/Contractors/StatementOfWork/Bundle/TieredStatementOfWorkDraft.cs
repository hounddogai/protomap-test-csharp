namespace Acme.Workforce.Contractors.StatementOfWork.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered statement of work draft is in its life cycle.</summary>
public enum TieredStatementOfWorkDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One tiered statement of work draft, with what the planners need to act on it.</summary>
public sealed class TieredStatementOfWorkDraft(string code, string name, bool pinned, int retries, int tenureMonths, string checksum, TieredStatementOfWorkDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int Retries { get; set; } = retries;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Checksum { get; set; } = checksum;
    public TieredStatementOfWorkDraftStatus Status { get; set; } = status;

    /// <summary>Starts a tiered statement of work draft in status planned.</summary>
    public static TieredStatementOfWorkDraft NewTieredStatementOfWorkDraft(string code, string name)
    {
        return new TieredStatementOfWorkDraft(code, name, false, 0, 0, "", TieredStatementOfWorkDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredStatementOfWorkDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredStatementOfWorkDraftStatus next)
    {
        switch (Status)
        {
            case TieredStatementOfWorkDraftStatus.Planned:
                return next == TieredStatementOfWorkDraftStatus.Completed;
            case TieredStatementOfWorkDraftStatus.Scheduled:
                switch (next)
                {
                    case TieredStatementOfWorkDraftStatus.InProgress:
                    case TieredStatementOfWorkDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case TieredStatementOfWorkDraftStatus.InProgress:
                return next == TieredStatementOfWorkDraftStatus.Completed || next == TieredStatementOfWorkDraftStatus.Cancelled;
            case TieredStatementOfWorkDraftStatus.Completed:
                switch (next)
                {
                    case TieredStatementOfWorkDraftStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredStatementOfWorkDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered statement of work draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Retries - (double)TenureMonths, 2);
    }

    public TieredStatementOfWorkDraft Copy()
    {
        return new TieredStatementOfWorkDraft(Code, Name, Pinned, Retries, TenureMonths, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
