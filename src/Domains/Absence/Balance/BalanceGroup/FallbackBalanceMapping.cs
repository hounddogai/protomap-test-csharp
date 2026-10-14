namespace Acme.Workforce.Absence.Balance.BalanceGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback balance mapping is in its life cycle.</summary>
public enum FallbackBalanceMappingStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A fallback balance mapping as the balance team keeps it.</summary>
public sealed class FallbackBalanceMapping(string code, string title, int updatedAt, double minimum, string ownerId, string externalRef, string category, double ceiling, FallbackBalanceMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public string OwnerId { get; set; } = ownerId;
    public string ExternalRef { get; set; } = externalRef;
    public string Category { get; set; } = category;
    public double Ceiling { get; set; } = ceiling;
    public FallbackBalanceMappingStatus Status { get; set; } = status;

    /// <summary>Starts a fallback balance mapping in status open.</summary>
    public static FallbackBalanceMapping NewFallbackBalanceMapping(string code, string title)
    {
        return new FallbackBalanceMapping(code, title, 0, 0.0, "", "", "", 0.0, FallbackBalanceMappingStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackBalanceMappingStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackBalanceMappingStatus next)
    {
        switch (Status)
        {
            case FallbackBalanceMappingStatus.Open:
                switch (next)
                {
                    case FallbackBalanceMappingStatus.Resolved:
                        return true;
                }
                return false;
            case FallbackBalanceMappingStatus.InReview:
                return next == FallbackBalanceMappingStatus.Reopened;
            case FallbackBalanceMappingStatus.Resolved:
                return next == FallbackBalanceMappingStatus.Reopened;
            case FallbackBalanceMappingStatus.Reopened:
                switch (next)
                {
                    case FallbackBalanceMappingStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackBalanceMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback balance mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Ceiling * Minimum, 2);
    }

    public FallbackBalanceMapping Copy()
    {
        return new FallbackBalanceMapping(
            Code,
            Title,
            UpdatedAt,
            Minimum,
            OwnerId,
            ExternalRef,
            Category,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
