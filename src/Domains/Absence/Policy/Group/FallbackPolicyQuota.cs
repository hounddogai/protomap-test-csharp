namespace Acme.Workforce.Absence.Policy.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy quota is in its life cycle.</summary>
public enum FallbackPolicyQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback policy quota as the policy team keeps it.</summary>
public sealed class FallbackPolicyQuota(string code, string name, double discount, int endsAt, double cost, List<string> tags, int durationMinutes, string jobCode, FallbackPolicyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public int EndsAt { get; set; } = endsAt;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string JobCode { get; set; } = jobCode;
    public FallbackPolicyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy quota in status open.</summary>
    public static FallbackPolicyQuota NewFallbackPolicyQuota(string code, string name)
    {
        return new FallbackPolicyQuota(
            code,
            name,
            0.0,
            0,
            0.0,
            new List<string>(),
            0,
            "",
            FallbackPolicyQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyQuotaStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyQuotaStatus.Open:
                switch (next)
                {
                    case FallbackPolicyQuotaStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackPolicyQuotaStatus.InReview:
                switch (next)
                {
                    case FallbackPolicyQuotaStatus.Reopened:
                        return true;
                }
                return false;
            case FallbackPolicyQuotaStatus.Resolved:
                return next == FallbackPolicyQuotaStatus.Archived;
            case FallbackPolicyQuotaStatus.Reopened:
                return next == FallbackPolicyQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double EffectiveValue()
    {
        return Math.Round(Cost * (double)DurationMinutes, 2);
    }

    public FallbackPolicyQuota Copy()
    {
        return new FallbackPolicyQuota(Code, Name, Discount, EndsAt, Cost, Tags, DurationMinutes, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
