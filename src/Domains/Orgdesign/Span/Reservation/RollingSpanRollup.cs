namespace Acme.Workforce.Orgdesign.Span.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling span rollup is in its life cycle.</summary>
public enum RollingSpanRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One rolling span rollup, with what the planners need to act on it.</summary>
public sealed class RollingSpanRollup(string code, string name, int tenureMonths, List<string> tags, int updatedAt, int endsAt, int expiresAt, string region, RollingSpanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int TenureMonths { get; set; } = tenureMonths;
    public List<string> Tags { get; set; } = tags;
    public int UpdatedAt { get; set; } = updatedAt;
    public int EndsAt { get; set; } = endsAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Region { get; set; } = region;
    public RollingSpanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a rolling span rollup in status open.</summary>
    public static RollingSpanRollup NewRollingSpanRollup(string code, string name)
    {
        return new RollingSpanRollup(code, name, 0, new List<string>(), 0, 0, 0, "", RollingSpanRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingSpanRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingSpanRollupStatus next)
    {
        switch (Status)
        {
            case RollingSpanRollupStatus.Open:
                switch (next)
                {
                    case RollingSpanRollupStatus.InReview:
                    case RollingSpanRollupStatus.Resolved:
                        return true;
                }
                return false;
            case RollingSpanRollupStatus.InReview:
                switch (next)
                {
                    case RollingSpanRollupStatus.Resolved:
                    case RollingSpanRollupStatus.Archived:
                        return true;
                }
                return false;
            case RollingSpanRollupStatus.Resolved:
                return next == RollingSpanRollupStatus.Archived;
            case RollingSpanRollupStatus.Reopened:
                switch (next)
                {
                    case RollingSpanRollupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingSpanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling span rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)ExpiresAt + (double)UpdatedAt, 2);
    }

    public RollingSpanRollup Copy()
    {
        return new RollingSpanRollup(Code, Name, TenureMonths, Tags, UpdatedAt, EndsAt, ExpiresAt, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
