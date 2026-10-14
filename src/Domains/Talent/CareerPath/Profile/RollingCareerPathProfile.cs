namespace Acme.Workforce.Talent.CareerPath.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling career path profile is in its life cycle.</summary>
public enum RollingCareerPathProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A rolling career path profile. Its code is unique within a tenant.</summary>
public sealed class RollingCareerPathProfile(string code, string name, int attempts, string jobCode, int durationMinutes, int windowDays, string timezone, string source, RollingCareerPathProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int WindowDays { get; set; } = windowDays;
    public string Timezone { get; set; } = timezone;
    public string Source { get; set; } = source;
    public RollingCareerPathProfileStatus Status { get; set; } = status;

    /// <summary>Starts a rolling career path profile in status open.</summary>
    public static RollingCareerPathProfile NewRollingCareerPathProfile(string code, string name)
    {
        return new RollingCareerPathProfile(code, name, 0, "", 0, 0, "", "", RollingCareerPathProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingCareerPathProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingCareerPathProfileStatus next)
    {
        switch (Status)
        {
            case RollingCareerPathProfileStatus.Open:
                switch (next)
                {
                    case RollingCareerPathProfileStatus.Reopened:
                        return true;
                }
                return false;
            case RollingCareerPathProfileStatus.InReview:
                return next == RollingCareerPathProfileStatus.Resolved;
            case RollingCareerPathProfileStatus.Resolved:
                return next == RollingCareerPathProfileStatus.Archived;
            case RollingCareerPathProfileStatus.Reopened:
                switch (next)
                {
                    case RollingCareerPathProfileStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingCareerPathProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling career path profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)DurationMinutes + (double)Attempts, 2);
    }

    public RollingCareerPathProfile Copy()
    {
        return new RollingCareerPathProfile(
            Code,
            Name,
            Attempts,
            JobCode,
            DurationMinutes,
            WindowDays,
            Timezone,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
