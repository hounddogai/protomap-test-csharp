namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat agreement is in its life cycle.</summary>
public enum LicenseSeatAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One license seat agreement, with what the planners need to act on it.</summary>
public sealed class LicenseSeatAgreement(string code, string name, int durationMinutes, string grade, double cost, string currency, double maximum, double weight, string country, string unit, LicenseSeatAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Grade { get; set; } = grade;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public double Maximum { get; set; } = maximum;
    public double Weight { get; set; } = weight;
    public string Country { get; set; } = country;
    public string Unit { get; set; } = unit;
    public LicenseSeatAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a license seat agreement in status planned.</summary>
    public static LicenseSeatAgreement NewLicenseSeatAgreement(string code, string name)
    {
        return new LicenseSeatAgreement(
            code,
            name,
            0,
            "",
            0.0,
            "",
            0.0,
            0.0,
            "",
            "",
            LicenseSeatAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatAgreementStatus next)
    {
        switch (Status)
        {
            case LicenseSeatAgreementStatus.Planned:
                return next == LicenseSeatAgreementStatus.Cancelled;
            case LicenseSeatAgreementStatus.Scheduled:
                return next == LicenseSeatAgreementStatus.Cancelled;
            case LicenseSeatAgreementStatus.InProgress:
                return next == LicenseSeatAgreementStatus.Completed;
            case LicenseSeatAgreementStatus.Completed:
                return next == LicenseSeatAgreementStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LicenseSeatAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Maximum + (double)DurationMinutes, 2);
    }

    public LicenseSeatAgreement Copy()
    {
        return new LicenseSeatAgreement(
            Code,
            Name,
            DurationMinutes,
            Grade,
            Cost,
            Currency,
            Maximum,
            Weight,
            Country,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
