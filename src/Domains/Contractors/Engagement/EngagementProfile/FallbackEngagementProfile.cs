namespace Acme.Workforce.Contractors.Engagement.EngagementProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback engagement profile is in its life cycle.</summary>
public enum FallbackEngagementProfileStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback engagement profile. Its code is unique within a tenant.</summary>
public sealed class FallbackEngagementProfile(string code, string name, string grade, int startsAt, double cost, int limit, string jobCode, int createdAt, int windowDays, int capacity, FallbackEngagementProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int StartsAt { get; set; } = startsAt;
    public double Cost { get; set; } = cost;
    public int Limit { get; set; } = limit;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public int Capacity { get; set; } = capacity;
    public FallbackEngagementProfileStatus Status { get; set; } = status;

    /// <summary>Starts a fallback engagement profile in status open.</summary>
    public static FallbackEngagementProfile NewFallbackEngagementProfile(string code, string name)
    {
        return new FallbackEngagementProfile(
            code,
            name,
            "",
            0,
            0.0,
            0,
            "",
            0,
            0,
            0,
            FallbackEngagementProfileStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackEngagementProfileStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackEngagementProfileStatus next)
    {
        switch (Status)
        {
            case FallbackEngagementProfileStatus.Open:
                switch (next)
                {
                    case FallbackEngagementProfileStatus.Resolved:
                    case FallbackEngagementProfileStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackEngagementProfileStatus.InReview:
                switch (next)
                {
                    case FallbackEngagementProfileStatus.Reopened:
                    case FallbackEngagementProfileStatus.Archived:
                        return true;
                }
                return false;
            case FallbackEngagementProfileStatus.Resolved:
                switch (next)
                {
                    case FallbackEngagementProfileStatus.Reopened:
                    case FallbackEngagementProfileStatus.Archived:
                        return true;
                }
                return false;
            case FallbackEngagementProfileStatus.Reopened:
                return next == FallbackEngagementProfileStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackEngagementProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback engagement profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Limit + (double)CreatedAt, 2);
    }

    public FallbackEngagementProfile Copy()
    {
        return new FallbackEngagementProfile(
            Code,
            Name,
            Grade,
            StartsAt,
            Cost,
            Limit,
            JobCode,
            CreatedAt,
            WindowDays,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
