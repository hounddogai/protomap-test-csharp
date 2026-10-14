namespace Acme.Workforce.Contractors.StatementOfWork.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work variance is in its life cycle.</summary>
public enum StatementOfWorkVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A statement of work variance. Its code is unique within a tenant.</summary>
public sealed class StatementOfWorkVariance(string code, string title, string country, string category, string region, double amount, StatementOfWorkVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Country { get; set; } = country;
    public string Category { get; set; } = category;
    public string Region { get; set; } = region;
    public double Amount { get; set; } = amount;
    public StatementOfWorkVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work variance in status created.</summary>
    public static StatementOfWorkVariance NewStatementOfWorkVariance(string code, string title)
    {
        return new StatementOfWorkVariance(code, title, "", "", "", 0.0, StatementOfWorkVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkVarianceStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkVarianceStatus.Created:
                return next == StatementOfWorkVarianceStatus.Published || next == StatementOfWorkVarianceStatus.Retired;
            case StatementOfWorkVarianceStatus.Validated:
                switch (next)
                {
                    case StatementOfWorkVarianceStatus.Published:
                        return true;
                }
                return false;
            case StatementOfWorkVarianceStatus.Published:
                return next == StatementOfWorkVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StatementOfWorkVariance Copy()
    {
        return new StatementOfWorkVariance(Code, Title, Country, Category, Region, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
