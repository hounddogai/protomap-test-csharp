namespace Acme.Workforce.Absence.ReturnPlan.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan agreement is in its life cycle.</summary>
public enum ReturnPlanAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a return plan agreement and its current status.</summary>
public sealed class ReturnPlanAgreement(string code, string label, double discount, double rating, string locale, double ceiling, string notes, ReturnPlanAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public double Rating { get; set; } = rating;
    public string Locale { get; set; } = locale;
    public double Ceiling { get; set; } = ceiling;
    public string Notes { get; set; } = notes;
    public ReturnPlanAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a return plan agreement in status created.</summary>
    public static ReturnPlanAgreement NewReturnPlanAgreement(string code, string label)
    {
        return new ReturnPlanAgreement(code, label, 0.0, 0.0, "", 0.0, "", ReturnPlanAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanAgreementStatus next)
    {
        switch (Status)
        {
            case ReturnPlanAgreementStatus.Created:
                switch (next)
                {
                    case ReturnPlanAgreementStatus.Retired:
                        return true;
                }
                return false;
            case ReturnPlanAgreementStatus.Validated:
                return next == ReturnPlanAgreementStatus.Published || next == ReturnPlanAgreementStatus.Retired;
            case ReturnPlanAgreementStatus.Published:
                return next == ReturnPlanAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ReturnPlanAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetVolume()
    {
        return Math.Round(Rating + Discount, 2);
    }

    public ReturnPlanAgreement Copy()
    {
        return new ReturnPlanAgreement(Code, Label, Discount, Rating, Locale, Ceiling, Notes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
