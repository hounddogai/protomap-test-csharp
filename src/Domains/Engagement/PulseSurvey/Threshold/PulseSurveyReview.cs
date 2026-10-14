namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey review is in its life cycle.</summary>
public enum PulseSurveyReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a pulse survey review and its current status.</summary>
public sealed class PulseSurveyReview(string code, string label, string skill, string summary, int revision, string notes, string costCenter, int priority, string locale, string source, double rate, PulseSurveyReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string Summary { get; set; } = summary;
    public int Revision { get; set; } = revision;
    public string Notes { get; set; } = notes;
    public string CostCenter { get; set; } = costCenter;
    public int Priority { get; set; } = priority;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public double Rate { get; set; } = rate;
    public PulseSurveyReviewStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey review in status draft.</summary>
    public static PulseSurveyReview NewPulseSurveyReview(string code, string label)
    {
        return new PulseSurveyReview(code, label, "", "", 0, "", "", 0, "", "", 0.0, PulseSurveyReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyReviewStatus next)
    {
        switch (Status)
        {
            case PulseSurveyReviewStatus.Draft:
                return next == PulseSurveyReviewStatus.Active || next == PulseSurveyReviewStatus.Suspended;
            case PulseSurveyReviewStatus.Pending:
                return next == PulseSurveyReviewStatus.Suspended || next == PulseSurveyReviewStatus.Closed;
            case PulseSurveyReviewStatus.Active:
                return next == PulseSurveyReviewStatus.Suspended;
            case PulseSurveyReviewStatus.Suspended:
                switch (next)
                {
                    case PulseSurveyReviewStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetExposure()
    {
        return Math.Round((double)Revision * (double)Priority, 2);
    }

    public PulseSurveyReview Copy()
    {
        return new PulseSurveyReview(
            Code,
            Label,
            Skill,
            Summary,
            Revision,
            Notes,
            CostCenter,
            Priority,
            Locale,
            Source,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
