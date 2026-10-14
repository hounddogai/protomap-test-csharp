namespace Acme.Workforce.Talent.CareerPath.PathRule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual career path digest is in its life cycle.</summary>
public enum ManualCareerPathDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a manual career path digest and its current status.</summary>
public sealed class ManualCareerPathDigest(string code, string label, string locale, string source, int priority, string title, string team, double score, string badge, string unit, string notes, ManualCareerPathDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Locale { get; set; } = locale;
    public string Source { get; set; } = source;
    public int Priority { get; set; } = priority;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public double Score { get; set; } = score;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public string Notes { get; set; } = notes;
    public ManualCareerPathDigestStatus Status { get; set; } = status;

    /// <summary>Starts a manual career path digest in status requested.</summary>
    public static ManualCareerPathDigest NewManualCareerPathDigest(string code, string label)
    {
        return new ManualCareerPathDigest(
            code,
            label,
            "",
            "",
            0,
            "",
            "",
            0.0,
            "",
            "",
            "",
            ManualCareerPathDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualCareerPathDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualCareerPathDigestStatus next)
    {
        switch (Status)
        {
            case ManualCareerPathDigestStatus.Requested:
                switch (next)
                {
                    case ManualCareerPathDigestStatus.Granted:
                        return true;
                }
                return false;
            case ManualCareerPathDigestStatus.Granted:
                switch (next)
                {
                    case ManualCareerPathDigestStatus.Expired:
                        return true;
                }
                return false;
            case ManualCareerPathDigestStatus.Revoked:
                return next == ManualCareerPathDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(ManualCareerPathDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual career path digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Score * (double)Priority, 2);
    }

    public ManualCareerPathDigest Copy()
    {
        return new ManualCareerPathDigest(
            Code,
            Label,
            Locale,
            Source,
            Priority,
            Title,
            Team,
            Score,
            Badge,
            Unit,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
