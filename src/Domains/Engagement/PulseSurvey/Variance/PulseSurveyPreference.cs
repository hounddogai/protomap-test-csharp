namespace Acme.Workforce.Engagement.PulseSurvey.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey preference is in its life cycle.</summary>
public enum PulseSurveyPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a pulse survey preference and its current status.</summary>
public sealed class PulseSurveyPreference(string code, string label, int revision, int quantity, double score, string segment, string country, PulseSurveyPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Quantity { get; set; } = quantity;
    public double Score { get; set; } = score;
    public string Segment { get; set; } = segment;
    public string Country { get; set; } = country;
    public PulseSurveyPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey preference in status idle.</summary>
    public static PulseSurveyPreference NewPulseSurveyPreference(string code, string label)
    {
        return new PulseSurveyPreference(code, label, 0, 0, 0.0, "", "", PulseSurveyPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyPreferenceStatus next)
    {
        switch (Status)
        {
            case PulseSurveyPreferenceStatus.Idle:
                return next == PulseSurveyPreferenceStatus.Draining || next == PulseSurveyPreferenceStatus.Stopped;
            case PulseSurveyPreferenceStatus.Warming:
                return next == PulseSurveyPreferenceStatus.Ready || next == PulseSurveyPreferenceStatus.Stopped;
            case PulseSurveyPreferenceStatus.Ready:
                return next == PulseSurveyPreferenceStatus.Draining || next == PulseSurveyPreferenceStatus.Stopped;
            case PulseSurveyPreferenceStatus.Draining:
                switch (next)
                {
                    case PulseSurveyPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Revision - Score, 2);
    }

    public PulseSurveyPreference Copy()
    {
        return new PulseSurveyPreference(Code, Label, Revision, Quantity, Score, Segment, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
