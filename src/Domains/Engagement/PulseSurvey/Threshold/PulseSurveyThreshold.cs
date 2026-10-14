namespace Acme.Workforce.Engagement.PulseSurvey.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey threshold is in its life cycle.</summary>
public enum PulseSurveyThresholdStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a pulse survey threshold and its current status.</summary>
public sealed class PulseSurveyThreshold(string code, string label, double threshold, int attempts, string salaryBand, PulseSurveyThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public int Attempts { get; set; } = attempts;
    public string SalaryBand { get; set; } = salaryBand;
    public PulseSurveyThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey threshold in status open.</summary>
    public static PulseSurveyThreshold NewPulseSurveyThreshold(string code, string label)
    {
        return new PulseSurveyThreshold(code, label, 0.0, 0, "", PulseSurveyThresholdStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyThresholdStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyThresholdStatus next)
    {
        switch (Status)
        {
            case PulseSurveyThresholdStatus.Open:
                switch (next)
                {
                    case PulseSurveyThresholdStatus.InReview:
                        return true;
                }
                return false;
            case PulseSurveyThresholdStatus.InReview:
                return next == PulseSurveyThresholdStatus.Resolved || next == PulseSurveyThresholdStatus.Archived;
            case PulseSurveyThresholdStatus.Resolved:
                return next == PulseSurveyThresholdStatus.Reopened;
            case PulseSurveyThresholdStatus.Reopened:
                return next == PulseSurveyThresholdStatus.Archived;
        }
        return false;
    }

    public void MoveTo(PulseSurveyThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Threshold * (double)Attempts, 2);
    }

    public PulseSurveyThreshold Copy()
    {
        return new PulseSurveyThreshold(Code, Label, Threshold, Attempts, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
