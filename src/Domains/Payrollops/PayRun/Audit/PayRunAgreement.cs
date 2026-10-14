namespace Acme.Workforce.Payrollops.PayRun.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay run agreement is in its life cycle.</summary>
public enum PayRunAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One pay run agreement, with what the planners need to act on it.</summary>
public sealed class PayRunAgreement(string code, string name, double rating, string category, string ownerId, PayRunAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public string OwnerId { get; set; } = ownerId;
    public PayRunAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a pay run agreement in status created.</summary>
    public static PayRunAgreement NewPayRunAgreement(string code, string name)
    {
        return new PayRunAgreement(code, name, 0.0, "", "", PayRunAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayRunAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayRunAgreementStatus next)
    {
        switch (Status)
        {
            case PayRunAgreementStatus.Created:
                return next == PayRunAgreementStatus.Retired;
            case PayRunAgreementStatus.Validated:
                return next == PayRunAgreementStatus.Retired;
            case PayRunAgreementStatus.Published:
                return next == PayRunAgreementStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PayRunAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay run agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Rating;
    }

    public PayRunAgreement Copy()
    {
        return new PayRunAgreement(Code, Name, Rating, Category, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
