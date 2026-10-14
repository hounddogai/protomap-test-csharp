namespace Acme.Workforce.Compliance.Certification.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy certification digest is in its life cycle.</summary>
public enum LegacyCertificationDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One legacy certification digest, with what the planners need to act on it.</summary>
public sealed class LegacyCertificationDigest(string code, string name, int updatedAt, double score, string grade, double rating, string currency, int attempts, string notes, int leaveDays, LegacyCertificationDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Score { get; set; } = score;
    public string Grade { get; set; } = grade;
    public double Rating { get; set; } = rating;
    public string Currency { get; set; } = currency;
    public int Attempts { get; set; } = attempts;
    public string Notes { get; set; } = notes;
    public int LeaveDays { get; set; } = leaveDays;
    public LegacyCertificationDigestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy certification digest in status open.</summary>
    public static LegacyCertificationDigest NewLegacyCertificationDigest(string code, string name)
    {
        return new LegacyCertificationDigest(
            code,
            name,
            0,
            0.0,
            "",
            0.0,
            "",
            0,
            "",
            0,
            LegacyCertificationDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyCertificationDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyCertificationDigestStatus next)
    {
        switch (Status)
        {
            case LegacyCertificationDigestStatus.Open:
                switch (next)
                {
                    case LegacyCertificationDigestStatus.Resolved:
                        return true;
                }
                return false;
            case LegacyCertificationDigestStatus.InReview:
                switch (next)
                {
                    case LegacyCertificationDigestStatus.Resolved:
                    case LegacyCertificationDigestStatus.Archived:
                        return true;
                }
                return false;
            case LegacyCertificationDigestStatus.Resolved:
                return next == LegacyCertificationDigestStatus.Archived;
            case LegacyCertificationDigestStatus.Reopened:
                return next == LegacyCertificationDigestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LegacyCertificationDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy certification digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round((double)LeaveDays * Rating, 2);
    }

    public LegacyCertificationDigest Copy()
    {
        return new LegacyCertificationDigest(
            Code,
            Name,
            UpdatedAt,
            Score,
            Grade,
            Rating,
            Currency,
            Attempts,
            Notes,
            LeaveDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
