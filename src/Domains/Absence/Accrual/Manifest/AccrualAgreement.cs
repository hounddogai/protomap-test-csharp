namespace Acme.Workforce.Absence.Accrual.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a accrual agreement is in its life cycle.</summary>
public enum AccrualAgreementStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A accrual agreement. Its code is unique within a tenant.</summary>
public sealed class AccrualAgreement(string code, string title, int endsAt, double floorValue, int revision, bool pinned, AccrualAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public double FloorValue { get; set; } = floorValue;
    public int Revision { get; set; } = revision;
    public bool Pinned { get; set; } = pinned;
    public AccrualAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a accrual agreement in status open.</summary>
    public static AccrualAgreement NewAccrualAgreement(string code, string title)
    {
        return new AccrualAgreement(code, title, 0, 0.0, 0, false, AccrualAgreementStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AccrualAgreementStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AccrualAgreementStatus next)
    {
        switch (Status)
        {
            case AccrualAgreementStatus.Open:
                switch (next)
                {
                    case AccrualAgreementStatus.InReview:
                    case AccrualAgreementStatus.Resolved:
                        return true;
                }
                return false;
            case AccrualAgreementStatus.InReview:
                switch (next)
                {
                    case AccrualAgreementStatus.Reopened:
                    case AccrualAgreementStatus.Archived:
                        return true;
                }
                return false;
            case AccrualAgreementStatus.Resolved:
                return next == AccrualAgreementStatus.Reopened || next == AccrualAgreementStatus.Archived;
            case AccrualAgreementStatus.Reopened:
                switch (next)
                {
                    case AccrualAgreementStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AccrualAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"accrual agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Revision + (double)EndsAt, 2);
    }

    public AccrualAgreement Copy()
    {
        return new AccrualAgreement(Code, Title, EndsAt, FloorValue, Revision, Pinned, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
