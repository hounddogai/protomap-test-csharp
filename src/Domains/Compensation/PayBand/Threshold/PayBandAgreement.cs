namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band agreement is in its life cycle.</summary>
public enum PayBandAgreementStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a pay band agreement and its current status.</summary>
public sealed class PayBandAgreement(string code, string title, string locale, double weight, int retries, PayBandAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public double Weight { get; set; } = weight;
    public int Retries { get; set; } = retries;
    public PayBandAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pay band agreement in status requested.</summary>
    public static PayBandAgreement NewPayBandAgreement(string code, string title)
    {
        return new PayBandAgreement(code, title, "", 0.0, 0, PayBandAgreementStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandAgreementStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandAgreementStatus next)
    {
        switch (Status)
        {
            case PayBandAgreementStatus.Requested:
                switch (next)
                {
                    case PayBandAgreementStatus.Granted:
                    case PayBandAgreementStatus.Expired:
                        return true;
                }
                return false;
            case PayBandAgreementStatus.Granted:
                switch (next)
                {
                    case PayBandAgreementStatus.Revoked:
                    case PayBandAgreementStatus.Expired:
                        return true;
                }
                return false;
            case PayBandAgreementStatus.Revoked:
                return next == PayBandAgreementStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PayBandAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)Retries + Weight, 2);
    }

    public PayBandAgreement Copy()
    {
        return new PayBandAgreement(Code, Title, Locale, Weight, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
