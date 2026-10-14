namespace Acme.Workforce.Contractors.StatementOfWork.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work quota is in its life cycle.</summary>
public enum StatementOfWorkQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a statement of work quota and its current status.</summary>
public sealed class StatementOfWorkQuota(string code, string label, string source, string contactEmail, bool archived, string salaryBand, string region, string reviewer, double score, StatementOfWorkQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Archived { get; set; } = archived;
    public string SalaryBand { get; set; } = salaryBand;
    public string Region { get; set; } = region;
    public string Reviewer { get; set; } = reviewer;
    public double Score { get; set; } = score;
    public StatementOfWorkQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work quota in status draft.</summary>
    public static StatementOfWorkQuota NewStatementOfWorkQuota(string code, string label)
    {
        return new StatementOfWorkQuota(
            code,
            label,
            "",
            "",
            false,
            "",
            "",
            "",
            0.0,
            StatementOfWorkQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkQuotaStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkQuotaStatus.Draft:
                return next == StatementOfWorkQuotaStatus.Active || next == StatementOfWorkQuotaStatus.Suspended;
            case StatementOfWorkQuotaStatus.Pending:
                return next == StatementOfWorkQuotaStatus.Active || next == StatementOfWorkQuotaStatus.Closed;
            case StatementOfWorkQuotaStatus.Active:
                switch (next)
                {
                    case StatementOfWorkQuotaStatus.Suspended:
                        return true;
                }
                return false;
            case StatementOfWorkQuotaStatus.Suspended:
                return next == StatementOfWorkQuotaStatus.Closed;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public StatementOfWorkQuota Copy()
    {
        return new StatementOfWorkQuota(
            Code,
            Label,
            Source,
            ContactEmail,
            Archived,
            SalaryBand,
            Region,
            Reviewer,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
