namespace Acme.Workforce.Engagement.PulseSurvey.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient pulse survey manifest is in its life cycle.</summary>
public enum LenientPulseSurveyManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a lenient pulse survey manifest and its current status.</summary>
public sealed class LenientPulseSurveyManifest(string code, string label, double amount, double ceiling, double minimum, string managerId, bool visible, bool active, int sequence, double rate, LenientPulseSurveyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public double Ceiling { get; set; } = ceiling;
    public double Minimum { get; set; } = minimum;
    public string ManagerId { get; set; } = managerId;
    public bool Visible { get; set; } = visible;
    public bool Active { get; set; } = active;
    public int Sequence { get; set; } = sequence;
    public double Rate { get; set; } = rate;
    public LenientPulseSurveyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a lenient pulse survey manifest in status new.</summary>
    public static LenientPulseSurveyManifest NewLenientPulseSurveyManifest(string code, string label)
    {
        return new LenientPulseSurveyManifest(
            code,
            label,
            0.0,
            0.0,
            0.0,
            "",
            false,
            false,
            0,
            0.0,
            LenientPulseSurveyManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientPulseSurveyManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientPulseSurveyManifestStatus next)
    {
        switch (Status)
        {
            case LenientPulseSurveyManifestStatus.New:
                switch (next)
                {
                    case LenientPulseSurveyManifestStatus.Done:
                    case LenientPulseSurveyManifestStatus.Failed:
                        return true;
                }
                return false;
            case LenientPulseSurveyManifestStatus.Queued:
                return next == LenientPulseSurveyManifestStatus.Failed;
            case LenientPulseSurveyManifestStatus.Running:
                return next == LenientPulseSurveyManifestStatus.Failed;
            case LenientPulseSurveyManifestStatus.Done:
                switch (next)
                {
                    case LenientPulseSurveyManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientPulseSurveyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient pulse survey manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Sequence - Amount, 2);
    }

    public LenientPulseSurveyManifest Copy()
    {
        return new LenientPulseSurveyManifest(
            Code,
            Label,
            Amount,
            Ceiling,
            Minimum,
            ManagerId,
            Visible,
            Active,
            Sequence,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
