namespace Acme.Workforce.Compliance.Certification.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal certification manifest is in its life cycle.</summary>
public enum SeasonalCertificationManifestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One seasonal certification manifest, with what the planners need to act on it.</summary>
public sealed class SeasonalCertificationManifest(string code, string label, double threshold, string team, int windowDays, int graceDays, SeasonalCertificationManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string Team { get; set; } = team;
    public int WindowDays { get; set; } = windowDays;
    public int GraceDays { get; set; } = graceDays;
    public SeasonalCertificationManifestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal certification manifest in status open.</summary>
    public static SeasonalCertificationManifest NewSeasonalCertificationManifest(string code, string label)
    {
        return new SeasonalCertificationManifest(
            code,
            label,
            0.0,
            "",
            0,
            0,
            SeasonalCertificationManifestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalCertificationManifestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalCertificationManifestStatus next)
    {
        switch (Status)
        {
            case SeasonalCertificationManifestStatus.Open:
                switch (next)
                {
                    case SeasonalCertificationManifestStatus.Resolved:
                        return true;
                }
                return false;
            case SeasonalCertificationManifestStatus.InReview:
                switch (next)
                {
                    case SeasonalCertificationManifestStatus.Resolved:
                    case SeasonalCertificationManifestStatus.Archived:
                        return true;
                }
                return false;
            case SeasonalCertificationManifestStatus.Resolved:
                return next == SeasonalCertificationManifestStatus.Reopened;
            case SeasonalCertificationManifestStatus.Reopened:
                return next == SeasonalCertificationManifestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SeasonalCertificationManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal certification manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"seasonal certification manifest"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)WindowDays - (double)GraceDays, 2);
    }

    public SeasonalCertificationManifest Copy()
    {
        return new SeasonalCertificationManifest(Code, Label, Threshold, Team, WindowDays, GraceDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
