namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate policy is in its life cycle.</summary>
public enum RatePolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rate policy and its current status.</summary>
public sealed class RatePolicy(string code, string label, string description, string team, int tenureMonths, string summary, RatePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public string Team { get; set; } = team;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Summary { get; set; } = summary;
    public RatePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a rate policy in status open.</summary>
    public static RatePolicy NewRatePolicy(string code, string label)
    {
        return new RatePolicy(code, label, "", "", 0, "", RatePolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RatePolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RatePolicyStatus next)
    {
        switch (Status)
        {
            case RatePolicyStatus.Open:
                return next == RatePolicyStatus.Reopened || next == RatePolicyStatus.Archived;
            case RatePolicyStatus.InReview:
                switch (next)
                {
                    case RatePolicyStatus.Resolved:
                    case RatePolicyStatus.Archived:
                        return true;
                }
                return false;
            case RatePolicyStatus.Resolved:
                switch (next)
                {
                    case RatePolicyStatus.Reopened:
                        return true;
                }
                return false;
            case RatePolicyStatus.Reopened:
                switch (next)
                {
                    case RatePolicyStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RatePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public RatePolicy Copy()
    {
        return new RatePolicy(Code, Label, Description, Team, TenureMonths, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
