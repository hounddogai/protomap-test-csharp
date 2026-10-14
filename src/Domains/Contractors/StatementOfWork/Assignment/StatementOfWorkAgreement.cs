namespace Acme.Workforce.Contractors.StatementOfWork.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work agreement is in its life cycle.</summary>
public enum StatementOfWorkAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a statement of work agreement and its current status.</summary>
public sealed class StatementOfWorkAgreement(string code, string title, int graceDays, double discount, string timezone, string region, StatementOfWorkAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int GraceDays { get; set; } = graceDays;
    public double Discount { get; set; } = discount;
    public string Timezone { get; set; } = timezone;
    public string Region { get; set; } = region;
    public StatementOfWorkAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work agreement in status planned.</summary>
    public static StatementOfWorkAgreement NewStatementOfWorkAgreement(string code, string title)
    {
        return new StatementOfWorkAgreement(code, title, 0, 0.0, "", "", StatementOfWorkAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkAgreementStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkAgreementStatus.Planned:
                switch (next)
                {
                    case StatementOfWorkAgreementStatus.Scheduled:
                        return true;
                }
                return false;
            case StatementOfWorkAgreementStatus.Scheduled:
                return next == StatementOfWorkAgreementStatus.Cancelled;
            case StatementOfWorkAgreementStatus.InProgress:
                return next == StatementOfWorkAgreementStatus.Completed;
            case StatementOfWorkAgreementStatus.Completed:
                return next == StatementOfWorkAgreementStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)GraceDays * Discount, 2);
    }

    public StatementOfWorkAgreement Copy()
    {
        return new StatementOfWorkAgreement(Code, Title, GraceDays, Discount, Timezone, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
