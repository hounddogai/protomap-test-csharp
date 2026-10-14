namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work draft is in its life cycle.</summary>
public enum StatementOfWorkDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a statement of work draft and its current status.</summary>
public sealed class StatementOfWorkDraft(string code, string label, int graceDays, bool approved, double score, bool pinned, string segment, double ratio, string name, string costCenter, StatementOfWorkDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public double Score { get; set; } = score;
    public bool Pinned { get; set; } = pinned;
    public string Segment { get; set; } = segment;
    public double Ratio { get; set; } = ratio;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public StatementOfWorkDraftStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work draft in status new.</summary>
    public static StatementOfWorkDraft NewStatementOfWorkDraft(string code, string label)
    {
        return new StatementOfWorkDraft(
            code,
            label,
            0,
            false,
            0.0,
            false,
            "",
            0.0,
            "",
            "",
            StatementOfWorkDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkDraftStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkDraftStatus.New:
                return next == StatementOfWorkDraftStatus.Running;
            case StatementOfWorkDraftStatus.Queued:
                return next == StatementOfWorkDraftStatus.Running || next == StatementOfWorkDraftStatus.Done;
            case StatementOfWorkDraftStatus.Running:
                return next == StatementOfWorkDraftStatus.Done;
            case StatementOfWorkDraftStatus.Done:
                return next == StatementOfWorkDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Ratio + (double)GraceDays, 2);
    }

    public StatementOfWorkDraft Copy()
    {
        return new StatementOfWorkDraft(
            Code,
            Label,
            GraceDays,
            Approved,
            Score,
            Pinned,
            Segment,
            Ratio,
            Name,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
