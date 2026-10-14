namespace Acme.Workforce.Contractors.StatementOfWork.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient statement of work bundle is in its life cycle.</summary>
public enum LenientStatementOfWorkBundleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a lenient statement of work bundle and its current status.</summary>
public sealed class LenientStatementOfWorkBundle(string code, string name, string label, string salaryBand, string reviewer, string description, int expiresAt, LenientStatementOfWorkBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Reviewer { get; set; } = reviewer;
    public string Description { get; set; } = description;
    public int ExpiresAt { get; set; } = expiresAt;
    public LenientStatementOfWorkBundleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient statement of work bundle in status planned.</summary>
    public static LenientStatementOfWorkBundle NewLenientStatementOfWorkBundle(string code, string name)
    {
        return new LenientStatementOfWorkBundle(
            code,
            name,
            "",
            "",
            "",
            "",
            0,
            LenientStatementOfWorkBundleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientStatementOfWorkBundleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientStatementOfWorkBundleStatus next)
    {
        switch (Status)
        {
            case LenientStatementOfWorkBundleStatus.Planned:
                return next == LenientStatementOfWorkBundleStatus.Cancelled;
            case LenientStatementOfWorkBundleStatus.Scheduled:
                switch (next)
                {
                    case LenientStatementOfWorkBundleStatus.InProgress:
                    case LenientStatementOfWorkBundleStatus.Completed:
                        return true;
                }
                return false;
            case LenientStatementOfWorkBundleStatus.InProgress:
                return next == LenientStatementOfWorkBundleStatus.Completed || next == LenientStatementOfWorkBundleStatus.Cancelled;
            case LenientStatementOfWorkBundleStatus.Completed:
                return next == LenientStatementOfWorkBundleStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientStatementOfWorkBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient statement of work bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return (double)ExpiresAt;
    }

    public LenientStatementOfWorkBundle Copy()
    {
        return new LenientStatementOfWorkBundle(
            Code,
            Name,
            Label,
            SalaryBand,
            Reviewer,
            Description,
            ExpiresAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
