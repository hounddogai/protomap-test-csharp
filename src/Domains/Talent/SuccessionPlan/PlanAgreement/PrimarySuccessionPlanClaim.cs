namespace Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary succession plan claim is in its life cycle.</summary>
public enum PrimarySuccessionPlanClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a primary succession plan claim and its current status.</summary>
public sealed class PrimarySuccessionPlanClaim(string code, string title, string locale, string source, double fte, PrimarySuccessionPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public double Fte { get; set; } = fte;
    public PrimarySuccessionPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a primary succession plan claim in status new.</summary>
    public static PrimarySuccessionPlanClaim NewPrimarySuccessionPlanClaim(string code, string title)
    {
        return new PrimarySuccessionPlanClaim(code, title, "", "", 0.0, PrimarySuccessionPlanClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySuccessionPlanClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySuccessionPlanClaimStatus next)
    {
        switch (Status)
        {
            case PrimarySuccessionPlanClaimStatus.New:
                switch (next)
                {
                    case PrimarySuccessionPlanClaimStatus.Failed:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanClaimStatus.Queued:
                switch (next)
                {
                    case PrimarySuccessionPlanClaimStatus.Running:
                    case PrimarySuccessionPlanClaimStatus.Failed:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanClaimStatus.Running:
                return next == PrimarySuccessionPlanClaimStatus.Failed;
            case PrimarySuccessionPlanClaimStatus.Done:
                return next == PrimarySuccessionPlanClaimStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimarySuccessionPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary succession plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary succession plan claim"} {Code}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public PrimarySuccessionPlanClaim Copy()
    {
        return new PrimarySuccessionPlanClaim(Code, Title, Locale, Source, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
