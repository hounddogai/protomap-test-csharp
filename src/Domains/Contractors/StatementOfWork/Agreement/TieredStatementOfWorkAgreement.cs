namespace Acme.Workforce.Contractors.StatementOfWork.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered statement of work agreement is in its life cycle.</summary>
public enum TieredStatementOfWorkAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a tiered statement of work agreement and its current status.</summary>
public sealed class TieredStatementOfWorkAgreement(string code, string name, int endsAt, int graceDays, int updatedAt, string managerId, TieredStatementOfWorkAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public int GraceDays { get; set; } = graceDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public string ManagerId { get; set; } = managerId;
    public TieredStatementOfWorkAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a tiered statement of work agreement in status open.</summary>
    public static TieredStatementOfWorkAgreement NewTieredStatementOfWorkAgreement(string code, string name)
    {
        return new TieredStatementOfWorkAgreement(
            code,
            name,
            0,
            0,
            0,
            "",
            TieredStatementOfWorkAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredStatementOfWorkAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredStatementOfWorkAgreementStatus next)
    {
        switch (Status)
        {
            case TieredStatementOfWorkAgreementStatus.Open:
                switch (next)
                {
                    case TieredStatementOfWorkAgreementStatus.Reopened:
                    case TieredStatementOfWorkAgreementStatus.Archived:
                        return true;
                }
                return false;
            case TieredStatementOfWorkAgreementStatus.InReview:
                return next == TieredStatementOfWorkAgreementStatus.Reopened;
            case TieredStatementOfWorkAgreementStatus.Resolved:
                switch (next)
                {
                    case TieredStatementOfWorkAgreementStatus.Reopened:
                    case TieredStatementOfWorkAgreementStatus.Archived:
                        return true;
                }
                return false;
            case TieredStatementOfWorkAgreementStatus.Reopened:
                switch (next)
                {
                    case TieredStatementOfWorkAgreementStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredStatementOfWorkAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered statement of work agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"tiered statement of work agreement"} {Code}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)UpdatedAt - (double)EndsAt, 2);
    }

    public TieredStatementOfWorkAgreement Copy()
    {
        return new TieredStatementOfWorkAgreement(Code, Name, EndsAt, GraceDays, UpdatedAt, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
