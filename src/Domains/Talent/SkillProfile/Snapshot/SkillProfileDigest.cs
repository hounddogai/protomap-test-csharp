namespace Acme.Workforce.Talent.SkillProfile.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile digest is in its life cycle.</summary>
public enum SkillProfileDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A skill profile digest. Its code is unique within a tenant.</summary>
public sealed class SkillProfileDigest(string code, string title, string managerId, int expiresAt, int graceDays, string channel, double budget, string timezone, string salaryBand, bool visible, SkillProfileDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public int ExpiresAt { get; set; } = expiresAt;
    public int GraceDays { get; set; } = graceDays;
    public string Channel { get; set; } = channel;
    public double Budget { get; set; } = budget;
    public string Timezone { get; set; } = timezone;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Visible { get; set; } = visible;
    public SkillProfileDigestStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile digest in status open.</summary>
    public static SkillProfileDigest NewSkillProfileDigest(string code, string title)
    {
        return new SkillProfileDigest(code, title, "", 0, 0, "", 0.0, "", "", false, SkillProfileDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileDigestStatus next)
    {
        switch (Status)
        {
            case SkillProfileDigestStatus.Open:
                switch (next)
                {
                    case SkillProfileDigestStatus.Resolved:
                    case SkillProfileDigestStatus.Reopened:
                        return true;
                }
                return false;
            case SkillProfileDigestStatus.InReview:
                switch (next)
                {
                    case SkillProfileDigestStatus.Resolved:
                    case SkillProfileDigestStatus.Reopened:
                        return true;
                }
                return false;
            case SkillProfileDigestStatus.Resolved:
                switch (next)
                {
                    case SkillProfileDigestStatus.Archived:
                        return true;
                }
                return false;
            case SkillProfileDigestStatus.Reopened:
                return next == SkillProfileDigestStatus.Archived;
        }
        return false;
    }

    public void MoveTo(SkillProfileDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"skill profile digest"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget * (double)GraceDays, 2);
    }

    public SkillProfileDigest Copy()
    {
        return new SkillProfileDigest(
            Code,
            Title,
            ManagerId,
            ExpiresAt,
            GraceDays,
            Channel,
            Budget,
            Timezone,
            SalaryBand,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
