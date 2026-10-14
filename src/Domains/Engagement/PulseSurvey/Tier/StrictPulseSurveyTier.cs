namespace Acme.Workforce.Engagement.PulseSurvey.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict pulse survey tier is in its life cycle.</summary>
public enum StrictPulseSurveyTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a strict pulse survey tier and its current status.</summary>
public sealed class StrictPulseSurveyTier(string code, string label, int leaveDays, double rate, string notes, string summary, StrictPulseSurveyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rate { get; set; } = rate;
    public string Notes { get; set; } = notes;
    public string Summary { get; set; } = summary;
    public StrictPulseSurveyTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict pulse survey tier in status draft.</summary>
    public static StrictPulseSurveyTier NewStrictPulseSurveyTier(string code, string label)
    {
        return new StrictPulseSurveyTier(code, label, 0, 0.0, "", "", StrictPulseSurveyTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictPulseSurveyTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictPulseSurveyTierStatus next)
    {
        switch (Status)
        {
            case StrictPulseSurveyTierStatus.Draft:
                return next == StrictPulseSurveyTierStatus.Active || next == StrictPulseSurveyTierStatus.Suspended;
            case StrictPulseSurveyTierStatus.Pending:
                switch (next)
                {
                    case StrictPulseSurveyTierStatus.Suspended:
                        return true;
                }
                return false;
            case StrictPulseSurveyTierStatus.Active:
                switch (next)
                {
                    case StrictPulseSurveyTierStatus.Suspended:
                    case StrictPulseSurveyTierStatus.Closed:
                        return true;
                }
                return false;
            case StrictPulseSurveyTierStatus.Suspended:
                switch (next)
                {
                    case StrictPulseSurveyTierStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictPulseSurveyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict pulse survey tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Rate + (double)LeaveDays, 2);
    }

    public StrictPulseSurveyTier Copy()
    {
        return new StrictPulseSurveyTier(Code, Label, LeaveDays, Rate, Notes, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
