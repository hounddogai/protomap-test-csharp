namespace Acme.Workforce.Talent.CareerPath.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy career path digest is in its life cycle.</summary>
public enum LegacyCareerPathDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A legacy career path digest. Its code is unique within a tenant.</summary>
public sealed class LegacyCareerPathDigest(string code, string title, string contactEmail, double maximum, double hours, LegacyCareerPathDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public double Maximum { get; set; } = maximum;
    public double Hours { get; set; } = hours;
    public LegacyCareerPathDigestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy career path digest in status new.</summary>
    public static LegacyCareerPathDigest NewLegacyCareerPathDigest(string code, string title)
    {
        return new LegacyCareerPathDigest(code, title, "", 0.0, 0.0, LegacyCareerPathDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyCareerPathDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyCareerPathDigestStatus next)
    {
        switch (Status)
        {
            case LegacyCareerPathDigestStatus.New:
                switch (next)
                {
                    case LegacyCareerPathDigestStatus.Queued:
                    case LegacyCareerPathDigestStatus.Done:
                        return true;
                }
                return false;
            case LegacyCareerPathDigestStatus.Queued:
                return next == LegacyCareerPathDigestStatus.Running;
            case LegacyCareerPathDigestStatus.Running:
                switch (next)
                {
                    case LegacyCareerPathDigestStatus.Done:
                        return true;
                }
                return false;
            case LegacyCareerPathDigestStatus.Done:
                return next == LegacyCareerPathDigestStatus.Failed;
        }
        return false;
    }

    public void MoveTo(LegacyCareerPathDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy career path digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Maximum + Hours, 2);
    }

    public LegacyCareerPathDigest Copy()
    {
        return new LegacyCareerPathDigest(Code, Title, ContactEmail, Maximum, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
