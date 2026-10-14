namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient succession plan agreement is in its life cycle.</summary>
public enum LenientSuccessionPlanAgreementStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A lenient succession plan agreement as the succession plan team keeps it.</summary>
public sealed class LenientSuccessionPlanAgreement(string code, string name, string timezone, int quantity, int retries, LenientSuccessionPlanAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Timezone { get; set; } = timezone;
    public int Quantity { get; set; } = quantity;
    public int Retries { get; set; } = retries;
    public LenientSuccessionPlanAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a lenient succession plan agreement in status created.</summary>
    public static LenientSuccessionPlanAgreement NewLenientSuccessionPlanAgreement(string code, string name)
    {
        return new LenientSuccessionPlanAgreement(
            code,
            name,
            "",
            0,
            0,
            LenientSuccessionPlanAgreementStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSuccessionPlanAgreementStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSuccessionPlanAgreementStatus next)
    {
        switch (Status)
        {
            case LenientSuccessionPlanAgreementStatus.Created:
                return next == LenientSuccessionPlanAgreementStatus.Validated || next == LenientSuccessionPlanAgreementStatus.Retired;
            case LenientSuccessionPlanAgreementStatus.Validated:
                switch (next)
                {
                    case LenientSuccessionPlanAgreementStatus.Published:
                        return true;
                }
                return false;
            case LenientSuccessionPlanAgreementStatus.Published:
                switch (next)
                {
                    case LenientSuccessionPlanAgreementStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientSuccessionPlanAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient succession plan agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"lenient succession plan agreement"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round((double)Quantity - (double)Retries, 2);
    }

    public LenientSuccessionPlanAgreement Copy()
    {
        return new LenientSuccessionPlanAgreement(Code, Name, Timezone, Quantity, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
