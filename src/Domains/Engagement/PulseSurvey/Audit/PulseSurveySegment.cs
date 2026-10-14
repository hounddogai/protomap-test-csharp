namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey segment is in its life cycle.</summary>
public enum PulseSurveySegmentStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One pulse survey segment, with what the planners need to act on it.</summary>
public sealed class PulseSurveySegment(string code, string label, string contactEmail, string summary, double fte, double weight, double hours, PulseSurveySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ContactEmail { get; set; } = contactEmail;
    public string Summary { get; set; } = summary;
    public double Fte { get; set; } = fte;
    public double Weight { get; set; } = weight;
    public double Hours { get; set; } = hours;
    public PulseSurveySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey segment in status open.</summary>
    public static PulseSurveySegment NewPulseSurveySegment(string code, string label)
    {
        return new PulseSurveySegment(code, label, "", "", 0.0, 0.0, 0.0, PulseSurveySegmentStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveySegmentStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveySegmentStatus next)
    {
        switch (Status)
        {
            case PulseSurveySegmentStatus.Open:
                switch (next)
                {
                    case PulseSurveySegmentStatus.Reopened:
                        return true;
                }
                return false;
            case PulseSurveySegmentStatus.InReview:
                return next == PulseSurveySegmentStatus.Archived;
            case PulseSurveySegmentStatus.Resolved:
                return next == PulseSurveySegmentStatus.Archived;
            case PulseSurveySegmentStatus.Reopened:
                switch (next)
                {
                    case PulseSurveySegmentStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte * Hours, 2);
    }

    public PulseSurveySegment Copy()
    {
        return new PulseSurveySegment(Code, Label, ContactEmail, Summary, Fte, Weight, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
