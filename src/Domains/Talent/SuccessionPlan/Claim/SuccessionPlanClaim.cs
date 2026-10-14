namespace Acme.Workforce.Talent.SuccessionPlan.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan claim is in its life cycle.</summary>
public enum SuccessionPlanClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One succession plan claim, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanClaim(string code, string title, double amount, int graceDays, int revision, SuccessionPlanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public int GraceDays { get; set; } = graceDays;
    public int Revision { get; set; } = revision;
    public SuccessionPlanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan claim in status created.</summary>
    public static SuccessionPlanClaim NewSuccessionPlanClaim(string code, string title)
    {
        return new SuccessionPlanClaim(code, title, 0.0, 0, 0, SuccessionPlanClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanClaimStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanClaimStatus.Created:
                return next == SuccessionPlanClaimStatus.Validated || next == SuccessionPlanClaimStatus.Published;
            case SuccessionPlanClaimStatus.Validated:
                switch (next)
                {
                    case SuccessionPlanClaimStatus.Published:
                    case SuccessionPlanClaimStatus.Retired:
                        return true;
                }
                return false;
            case SuccessionPlanClaimStatus.Published:
                return next == SuccessionPlanClaimStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Amount + (double)GraceDays, 2);
    }

    public SuccessionPlanClaim Copy()
    {
        return new SuccessionPlanClaim(Code, Title, Amount, GraceDays, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
