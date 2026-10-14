namespace Acme.Workforce.Talent.SuccessionPlan.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive succession plan policy is in its life cycle.</summary>
public enum AdaptiveSuccessionPlanPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a adaptive succession plan policy and its current status.</summary>
public sealed class AdaptiveSuccessionPlanPolicy(string code, string name, int limit, string currency, double minimum, double fte, string label, string reviewer, AdaptiveSuccessionPlanPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string Currency { get; set; } = currency;
    public double Minimum { get; set; } = minimum;
    public double Fte { get; set; } = fte;
    public string Label { get; set; } = label;
    public string Reviewer { get; set; } = reviewer;
    public AdaptiveSuccessionPlanPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive succession plan policy in status open.</summary>
    public static AdaptiveSuccessionPlanPolicy NewAdaptiveSuccessionPlanPolicy(string code, string name)
    {
        return new AdaptiveSuccessionPlanPolicy(
            code,
            name,
            0,
            "",
            0.0,
            0.0,
            "",
            "",
            AdaptiveSuccessionPlanPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveSuccessionPlanPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveSuccessionPlanPolicyStatus next)
    {
        switch (Status)
        {
            case AdaptiveSuccessionPlanPolicyStatus.Open:
                switch (next)
                {
                    case AdaptiveSuccessionPlanPolicyStatus.Resolved:
                    case AdaptiveSuccessionPlanPolicyStatus.Reopened:
                        return true;
                }
                return false;
            case AdaptiveSuccessionPlanPolicyStatus.InReview:
                return next == AdaptiveSuccessionPlanPolicyStatus.Resolved;
            case AdaptiveSuccessionPlanPolicyStatus.Resolved:
                return next == AdaptiveSuccessionPlanPolicyStatus.Reopened;
            case AdaptiveSuccessionPlanPolicyStatus.Reopened:
                return next == AdaptiveSuccessionPlanPolicyStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AdaptiveSuccessionPlanPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive succession plan policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedExposure()
    {
        return Math.Round(Fte * Minimum, 2);
    }

    public AdaptiveSuccessionPlanPolicy Copy()
    {
        return new AdaptiveSuccessionPlanPolicy(Code, Name, Limit, Currency, Minimum, Fte, Label, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
