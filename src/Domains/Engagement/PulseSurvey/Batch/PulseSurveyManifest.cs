namespace Acme.Workforce.Engagement.PulseSurvey.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pulse survey manifest is in its life cycle.</summary>
public enum PulseSurveyManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One pulse survey manifest, with what the planners need to act on it.</summary>
public sealed class PulseSurveyManifest(string code, string title, int revision, int attempts, string locale, double floorValue, double fte, PulseSurveyManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Revision { get; set; } = revision;
    public int Attempts { get; set; } = attempts;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public double Fte { get; set; } = fte;
    public PulseSurveyManifestStatus Status { get; set; } = status;

    /// <summary>Starts a pulse survey manifest in status new.</summary>
    public static PulseSurveyManifest NewPulseSurveyManifest(string code, string title)
    {
        return new PulseSurveyManifest(code, title, 0, 0, "", 0.0, 0.0, PulseSurveyManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PulseSurveyManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PulseSurveyManifestStatus next)
    {
        switch (Status)
        {
            case PulseSurveyManifestStatus.New:
                return next == PulseSurveyManifestStatus.Running || next == PulseSurveyManifestStatus.Done;
            case PulseSurveyManifestStatus.Queued:
                return next == PulseSurveyManifestStatus.Running || next == PulseSurveyManifestStatus.Done;
            case PulseSurveyManifestStatus.Running:
                switch (next)
                {
                    case PulseSurveyManifestStatus.Done:
                        return true;
                }
                return false;
            case PulseSurveyManifestStatus.Done:
                switch (next)
                {
                    case PulseSurveyManifestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PulseSurveyManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pulse survey manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)Attempts - FloorValue, 2);
    }

    public PulseSurveyManifest Copy()
    {
        return new PulseSurveyManifest(Code, Title, Revision, Attempts, Locale, FloorValue, Fte, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
