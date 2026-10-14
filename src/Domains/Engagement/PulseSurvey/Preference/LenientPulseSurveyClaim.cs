namespace Acme.Workforce.Engagement.PulseSurvey.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pulse survey claim is in its life cycle.</summary>
public enum LenientPulseSurveyClaimStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A lenient pulse survey claim as the pulse survey team keeps it.</summary>
public sealed class LenientPulseSurveyClaim(string code, string title, string description, string notes, int graceDays, double cost, bool active, LenientPulseSurveyClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Description { get; set; } = description;
    public string Notes { get; set; } = notes;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public bool Active { get; set; } = active;
    public LenientPulseSurveyClaimStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pulse survey claim in status open.</summary>
    public static LenientPulseSurveyClaim NewLenientPulseSurveyClaim(string code, string title)
    {
        return new LenientPulseSurveyClaim(code, title, "", "", 0, 0.0, false, LenientPulseSurveyClaimStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPulseSurveyClaimStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPulseSurveyClaimStatus next)
    {
        switch (Status)
        {
            case LenientPulseSurveyClaimStatus.Open:
                return next == LenientPulseSurveyClaimStatus.InReview || next == LenientPulseSurveyClaimStatus.Reopened;
            case LenientPulseSurveyClaimStatus.InReview:
                return next == LenientPulseSurveyClaimStatus.Resolved || next == LenientPulseSurveyClaimStatus.Reopened;
            case LenientPulseSurveyClaimStatus.Resolved:
                switch (next)
                {
                    case LenientPulseSurveyClaimStatus.Archived:
                        return true;
                }
                return false;
            case LenientPulseSurveyClaimStatus.Reopened:
                switch (next)
                {
                    case LenientPulseSurveyClaimStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPulseSurveyClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pulse survey claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)GraceDays + Cost, 2);
    }

    public LenientPulseSurveyClaim Copy()
    {
        return new LenientPulseSurveyClaim(Code, Title, Description, Notes, GraceDays, Cost, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
