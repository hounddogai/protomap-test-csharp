namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default garnishment agreement is in its life cycle.</summary>
public enum DefaultGarnishmentAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default garnishment agreement, with what the planners need to act on it.</summary>
public sealed class DefaultGarnishmentAgreement(string code, string title, double ratio, int limit, int durationMinutes, string country, string salaryBand, double minimum, DefaultGarnishmentAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public int Limit { get; set; } = limit;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Country { get; set; } = country;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public DefaultGarnishmentAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a default garnishment agreement in status requested.</summary>
    public static DefaultGarnishmentAgreement NewDefaultGarnishmentAgreement(string code, string title)
    {
        return new DefaultGarnishmentAgreement(
            code,
            title,
            0.0,
            0,
            0,
            "",
            "",
            0.0,
            DefaultGarnishmentAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultGarnishmentAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultGarnishmentAgreementStatus next)
    {
        switch (Status)
        {
            case DefaultGarnishmentAgreementStatus.Requested:
                return next == DefaultGarnishmentAgreementStatus.Granted;
            case DefaultGarnishmentAgreementStatus.Granted:
                switch (next)
                {
                    case DefaultGarnishmentAgreementStatus.Revoked:
                    case DefaultGarnishmentAgreementStatus.Expired:
                        return true;
                }
                return false;
            case DefaultGarnishmentAgreementStatus.Revoked:
                switch (next)
                {
                    case DefaultGarnishmentAgreementStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultGarnishmentAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default garnishment agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)Limit + (double)DurationMinutes, 2);
    }

    public DefaultGarnishmentAgreement Copy()
    {
        return new DefaultGarnishmentAgreement(
            Code,
            Title,
            Ratio,
            Limit,
            DurationMinutes,
            Country,
            SalaryBand,
            Minimum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
