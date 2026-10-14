namespace Acme.Workforce.Workplace.AccessBadge.BadgeEntry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling access badge agreement is in its life cycle.</summary>
public enum RollingAccessBadgeAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A rolling access badge agreement. Its code is unique within a tenant.</summary>
public sealed class RollingAccessBadgeAgreement(string code, string title, double maximum, int revision, string timezone, string unit, double rate, RollingAccessBadgeAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Maximum { get; set; } = maximum;
    public int Revision { get; set; } = revision;
    public string Timezone { get; set; } = timezone;
    public string Unit { get; set; } = unit;
    public double Rate { get; set; } = rate;
    public RollingAccessBadgeAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a rolling access badge agreement in status created.</summary>
    public static RollingAccessBadgeAgreement NewRollingAccessBadgeAgreement(string code, string title)
    {
        return new RollingAccessBadgeAgreement(
            code,
            title,
            0.0,
            0,
            "",
            "",
            0.0,
            RollingAccessBadgeAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingAccessBadgeAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingAccessBadgeAgreementStatus next)
    {
        switch (Status)
        {
            case RollingAccessBadgeAgreementStatus.Created:
                return next == RollingAccessBadgeAgreementStatus.Published || next == RollingAccessBadgeAgreementStatus.Retired;
            case RollingAccessBadgeAgreementStatus.Validated:
                switch (next)
                {
                    case RollingAccessBadgeAgreementStatus.Published:
                    case RollingAccessBadgeAgreementStatus.Retired:
                        return true;
                }
                return false;
            case RollingAccessBadgeAgreementStatus.Published:
                return next == RollingAccessBadgeAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RollingAccessBadgeAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling access badge agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Revision - Rate, 2);
    }

    public RollingAccessBadgeAgreement Copy()
    {
        return new RollingAccessBadgeAgreement(Code, Title, Maximum, Revision, Timezone, Unit, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
