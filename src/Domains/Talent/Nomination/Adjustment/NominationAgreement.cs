namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a nomination agreement is in its life cycle.</summary>
public enum NominationAgreementStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a nomination agreement and its current status.</summary>
public sealed class NominationAgreement(string code, string title, string currency, double fte, int createdAt, int revision, double weight, double rate, string location, NominationAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Currency { get; set; } = currency;
    public double Fte { get; set; } = fte;
    public int CreatedAt { get; set; } = createdAt;
    public int Revision { get; set; } = revision;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public string Location { get; set; } = location;
    public NominationAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a nomination agreement in status proposed.</summary>
    public static NominationAgreement NewNominationAgreement(string code, string title)
    {
        return new NominationAgreement(code, title, "", 0.0, 0, 0, 0.0, 0.0, "", NominationAgreementStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case NominationAgreementStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(NominationAgreementStatus next)
    {
        switch (Status)
        {
            case NominationAgreementStatus.Proposed:
                return next == NominationAgreementStatus.Rejected || next == NominationAgreementStatus.Withdrawn;
            case NominationAgreementStatus.Approved:
                switch (next)
                {
                    case NominationAgreementStatus.Rejected:
                    case NominationAgreementStatus.Withdrawn:
                        return true;
                }
                return false;
            case NominationAgreementStatus.Rejected:
                return next == NominationAgreementStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(NominationAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"nomination agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Revision * Rate, 2);
    }

    public NominationAgreement Copy()
    {
        return new NominationAgreement(
            Code,
            Title,
            Currency,
            Fte,
            CreatedAt,
            Revision,
            Weight,
            Rate,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
