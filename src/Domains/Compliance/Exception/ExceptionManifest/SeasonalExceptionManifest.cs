namespace Acme.Workforce.Compliance.Exception.ExceptionManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal exception manifest is in its life cycle.</summary>
public enum SeasonalExceptionManifestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One seasonal exception manifest, with what the planners need to act on it.</summary>
public sealed class SeasonalExceptionManifest(string code, string label, string description, double rating, int capacity, int windowDays, SeasonalExceptionManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Rating { get; set; } = rating;
    public int Capacity { get; set; } = capacity;
    public int WindowDays { get; set; } = windowDays;
    public SeasonalExceptionManifestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal exception manifest in status new.</summary>
    public static SeasonalExceptionManifest NewSeasonalExceptionManifest(string code, string label)
    {
        return new SeasonalExceptionManifest(code, label, "", 0.0, 0, 0, SeasonalExceptionManifestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalExceptionManifestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalExceptionManifestStatus next)
    {
        switch (Status)
        {
            case SeasonalExceptionManifestStatus.New:
                switch (next)
                {
                    case SeasonalExceptionManifestStatus.Running:
                        return true;
                }
                return false;
            case SeasonalExceptionManifestStatus.Queued:
                switch (next)
                {
                    case SeasonalExceptionManifestStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalExceptionManifestStatus.Running:
                switch (next)
                {
                    case SeasonalExceptionManifestStatus.Done:
                    case SeasonalExceptionManifestStatus.Failed:
                        return true;
                }
                return false;
            case SeasonalExceptionManifestStatus.Done:
                return next == SeasonalExceptionManifestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(SeasonalExceptionManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal exception manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double NetExposure()
    {
        return Math.Round(Rating * (double)Capacity, 2);
    }

    public SeasonalExceptionManifest Copy()
    {
        return new SeasonalExceptionManifest(Code, Label, Description, Rating, Capacity, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
