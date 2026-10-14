namespace Acme.Workforce.Payrollops.PayRun.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pay run agreement is in its life cycle.</summary>
public enum LenientPayRunAgreementStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A lenient pay run agreement. Its code is unique within a tenant.</summary>
public sealed class LenientPayRunAgreement(string code, string title, int tenureMonths, string badge, string description, LenientPayRunAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Badge { get; set; } = badge;
    public string Description { get; set; } = description;
    public LenientPayRunAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pay run agreement in status draft.</summary>
    public static LenientPayRunAgreement NewLenientPayRunAgreement(string code, string title)
    {
        return new LenientPayRunAgreement(code, title, 0, "", "", LenientPayRunAgreementStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPayRunAgreementStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPayRunAgreementStatus next)
    {
        switch (Status)
        {
            case LenientPayRunAgreementStatus.Draft:
                switch (next)
                {
                    case LenientPayRunAgreementStatus.Active:
                    case LenientPayRunAgreementStatus.Closed:
                        return true;
                }
                return false;
            case LenientPayRunAgreementStatus.Pending:
                switch (next)
                {
                    case LenientPayRunAgreementStatus.Suspended:
                    case LenientPayRunAgreementStatus.Closed:
                        return true;
                }
                return false;
            case LenientPayRunAgreementStatus.Active:
                return next == LenientPayRunAgreementStatus.Suspended;
            case LenientPayRunAgreementStatus.Suspended:
                return next == LenientPayRunAgreementStatus.Closed;
        }
        return false;
    }

    public void MoveTo(LenientPayRunAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pay run agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient pay run agreement"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public LenientPayRunAgreement Copy()
    {
        return new LenientPayRunAgreement(Code, Title, TenureMonths, Badge, Description, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
