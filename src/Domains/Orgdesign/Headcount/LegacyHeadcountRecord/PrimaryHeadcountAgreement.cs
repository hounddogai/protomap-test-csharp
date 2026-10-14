namespace Acme.Workforce.Orgdesign.Headcount.LegacyHeadcountRecord;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary headcount agreement is in its life cycle.</summary>
public enum PrimaryHeadcountAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary headcount agreement and its current status.</summary>
public sealed class PrimaryHeadcountAgreement(string code, string title, string ownerId, int createdAt, bool visible, PrimaryHeadcountAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public int CreatedAt { get; set; } = createdAt;
    public bool Visible { get; set; } = visible;
    public PrimaryHeadcountAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a primary headcount agreement in status created.</summary>
    public static PrimaryHeadcountAgreement NewPrimaryHeadcountAgreement(string code, string title)
    {
        return new PrimaryHeadcountAgreement(code, title, "", 0, false, PrimaryHeadcountAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryHeadcountAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryHeadcountAgreementStatus next)
    {
        switch (Status)
        {
            case PrimaryHeadcountAgreementStatus.Created:
                switch (next)
                {
                    case PrimaryHeadcountAgreementStatus.Published:
                    case PrimaryHeadcountAgreementStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryHeadcountAgreementStatus.Validated:
                switch (next)
                {
                    case PrimaryHeadcountAgreementStatus.Published:
                        return true;
                }
                return false;
            case PrimaryHeadcountAgreementStatus.Published:
                return next == PrimaryHeadcountAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PrimaryHeadcountAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary headcount agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public PrimaryHeadcountAgreement Copy()
    {
        return new PrimaryHeadcountAgreement(Code, Title, OwnerId, CreatedAt, Visible, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
