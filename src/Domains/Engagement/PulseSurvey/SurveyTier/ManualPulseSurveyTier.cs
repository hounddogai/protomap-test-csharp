namespace Acme.Workforce.Engagement.PulseSurvey.SurveyTier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual pulse survey tier is in its life cycle.</summary>
public enum ManualPulseSurveyTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One manual pulse survey tier, with what the planners need to act on it.</summary>
public sealed class ManualPulseSurveyTier(string code, string label, string title, string region, string externalRef, string source, string location, string unit, double amount, ManualPulseSurveyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string ExternalRef { get; set; } = externalRef;
    public string Source { get; set; } = source;
    public string Location { get; set; } = location;
    public string Unit { get; set; } = unit;
    public double Amount { get; set; } = amount;
    public ManualPulseSurveyTierStatus Status { get; set; } = status;

    /// <summary>Starts a manual pulse survey tier in status open.</summary>
    public static ManualPulseSurveyTier NewManualPulseSurveyTier(string code, string label)
    {
        return new ManualPulseSurveyTier(code, label, "", "", "", "", "", "", 0.0, ManualPulseSurveyTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualPulseSurveyTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualPulseSurveyTierStatus next)
    {
        switch (Status)
        {
            case ManualPulseSurveyTierStatus.Open:
                switch (next)
                {
                    case ManualPulseSurveyTierStatus.InReview:
                    case ManualPulseSurveyTierStatus.Resolved:
                        return true;
                }
                return false;
            case ManualPulseSurveyTierStatus.InReview:
                switch (next)
                {
                    case ManualPulseSurveyTierStatus.Resolved:
                    case ManualPulseSurveyTierStatus.Archived:
                        return true;
                }
                return false;
            case ManualPulseSurveyTierStatus.Resolved:
                switch (next)
                {
                    case ManualPulseSurveyTierStatus.Archived:
                        return true;
                }
                return false;
            case ManualPulseSurveyTierStatus.Reopened:
                switch (next)
                {
                    case ManualPulseSurveyTierStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualPulseSurveyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual pulse survey tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ManualPulseSurveyTier Copy()
    {
        return new ManualPulseSurveyTier(
            Code,
            Label,
            Title,
            Region,
            ExternalRef,
            Source,
            Location,
            Unit,
            Amount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
