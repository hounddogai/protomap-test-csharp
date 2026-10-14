namespace Acme.Workforce.Contractors.StatementOfWork.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy statement of work agreement is in its life cycle.</summary>
public enum LegacyStatementOfWorkAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One legacy statement of work agreement, with what the planners need to act on it.</summary>
public sealed class LegacyStatementOfWorkAgreement(string code, string title, string country, string category, double amount, int tenureMonths, double budget, LegacyStatementOfWorkAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public double Amount { get; set; } = amount;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Budget { get; set; } = budget;
    public LegacyStatementOfWorkAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a legacy statement of work agreement in status created.</summary>
    public static LegacyStatementOfWorkAgreement NewLegacyStatementOfWorkAgreement(string code, string title)
    {
        return new LegacyStatementOfWorkAgreement(
            code,
            title,
            "",
            "",
            0.0,
            0,
            0.0,
            LegacyStatementOfWorkAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyStatementOfWorkAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyStatementOfWorkAgreementStatus next)
    {
        switch (Status)
        {
            case LegacyStatementOfWorkAgreementStatus.Created:
                return next == LegacyStatementOfWorkAgreementStatus.Validated || next == LegacyStatementOfWorkAgreementStatus.Retired;
            case LegacyStatementOfWorkAgreementStatus.Validated:
                return next == LegacyStatementOfWorkAgreementStatus.Published || next == LegacyStatementOfWorkAgreementStatus.Retired;
            case LegacyStatementOfWorkAgreementStatus.Published:
                return next == LegacyStatementOfWorkAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(LegacyStatementOfWorkAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy statement of work agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Budget * (double)TenureMonths, 2);
    }

    public LegacyStatementOfWorkAgreement Copy()
    {
        return new LegacyStatementOfWorkAgreement(
            Code,
            Title,
            Country,
            Category,
            Amount,
            TenureMonths,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
