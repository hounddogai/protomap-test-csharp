namespace Acme.Workforce.Contractors.Offboarding.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a offboarding digest is in its life cycle.</summary>
public enum OffboardingDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One offboarding digest, with what the planners need to act on it.</summary>
public sealed class OffboardingDigest(string code, string title, int leaveDays, string summary, int attempts, OffboardingDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Summary { get; set; } = summary;
    public int Attempts { get; set; } = attempts;
    public OffboardingDigestStatus Status { get; set; } = status;

    /// <summary>Starts a offboarding digest in status open.</summary>
    public static OffboardingDigest NewOffboardingDigest(string code, string title)
    {
        return new OffboardingDigest(code, title, 0, "", 0, OffboardingDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OffboardingDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OffboardingDigestStatus next)
    {
        switch (Status)
        {
            case OffboardingDigestStatus.Open:
                switch (next)
                {
                    case OffboardingDigestStatus.InReview:
                        return true;
                }
                return false;
            case OffboardingDigestStatus.InReview:
                switch (next)
                {
                    case OffboardingDigestStatus.Archived:
                        return true;
                }
                return false;
            case OffboardingDigestStatus.Resolved:
                switch (next)
                {
                    case OffboardingDigestStatus.Reopened:
                    case OffboardingDigestStatus.Archived:
                        return true;
                }
                return false;
            case OffboardingDigestStatus.Reopened:
                return next == OffboardingDigestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(OffboardingDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"offboarding digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)LeaveDays * (double)Attempts, 2);
    }

    public OffboardingDigest Copy()
    {
        return new OffboardingDigest(Code, Title, LeaveDays, Summary, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
