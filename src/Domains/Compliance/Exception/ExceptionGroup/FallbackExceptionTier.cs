namespace Acme.Workforce.Compliance.Exception.ExceptionGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback exception tier is in its life cycle.</summary>
public enum FallbackExceptionTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One fallback exception tier, with what the planners need to act on it.</summary>
public sealed class FallbackExceptionTier(string code, string name, string costCenter, int createdAt, int attempts, string externalRef, string skill, int windowDays, double rating, string category, string label, FallbackExceptionTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public int CreatedAt { get; set; } = createdAt;
    public int Attempts { get; set; } = attempts;
    public string ExternalRef { get; set; } = externalRef;
    public string Skill { get; set; } = skill;
    public int WindowDays { get; set; } = windowDays;
    public double Rating { get; set; } = rating;
    public string Category { get; set; } = category;
    public string Label { get; set; } = label;
    public FallbackExceptionTierStatus Status { get; set; } = status;

    /// <summary>Starts a fallback exception tier in status open.</summary>
    public static FallbackExceptionTier NewFallbackExceptionTier(string code, string name)
    {
        return new FallbackExceptionTier(
            code,
            name,
            "",
            0,
            0,
            "",
            "",
            0,
            0.0,
            "",
            "",
            FallbackExceptionTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackExceptionTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackExceptionTierStatus next)
    {
        switch (Status)
        {
            case FallbackExceptionTierStatus.Open:
                switch (next)
                {
                    case FallbackExceptionTierStatus.Archived:
                        return true;
                }
                return false;
            case FallbackExceptionTierStatus.InReview:
                return next == FallbackExceptionTierStatus.Reopened;
            case FallbackExceptionTierStatus.Resolved:
                switch (next)
                {
                    case FallbackExceptionTierStatus.Reopened:
                    case FallbackExceptionTierStatus.Archived:
                        return true;
                }
                return false;
            case FallbackExceptionTierStatus.Reopened:
                return next == FallbackExceptionTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackExceptionTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback exception tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)WindowDays * (double)Attempts, 2);
    }

    public FallbackExceptionTier Copy()
    {
        return new FallbackExceptionTier(
            Code,
            Name,
            CostCenter,
            CreatedAt,
            Attempts,
            ExternalRef,
            Skill,
            WindowDays,
            Rating,
            Category,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
