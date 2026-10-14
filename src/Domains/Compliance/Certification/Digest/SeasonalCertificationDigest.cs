namespace Acme.Workforce.Compliance.Certification.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal certification digest is in its life cycle.</summary>
public enum SeasonalCertificationDigestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a seasonal certification digest and its current status.</summary>
public sealed class SeasonalCertificationDigest(string code, string label, string team, int durationMinutes, int priority, SeasonalCertificationDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int Priority { get; set; } = priority;
    public SeasonalCertificationDigestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal certification digest in status proposed.</summary>
    public static SeasonalCertificationDigest NewSeasonalCertificationDigest(string code, string label)
    {
        return new SeasonalCertificationDigest(code, label, "", 0, 0, SeasonalCertificationDigestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalCertificationDigestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalCertificationDigestStatus next)
    {
        switch (Status)
        {
            case SeasonalCertificationDigestStatus.Proposed:
                switch (next)
                {
                    case SeasonalCertificationDigestStatus.Approved:
                    case SeasonalCertificationDigestStatus.Rejected:
                        return true;
                }
                return false;
            case SeasonalCertificationDigestStatus.Approved:
                return next == SeasonalCertificationDigestStatus.Rejected || next == SeasonalCertificationDigestStatus.Withdrawn;
            case SeasonalCertificationDigestStatus.Rejected:
                return next == SeasonalCertificationDigestStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(SeasonalCertificationDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal certification digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetVolume()
    {
        return Math.Round((double)DurationMinutes * (double)Priority, 2);
    }

    public SeasonalCertificationDigest Copy()
    {
        return new SeasonalCertificationDigest(Code, Label, Team, DurationMinutes, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
