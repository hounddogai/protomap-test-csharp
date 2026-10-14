namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback position quota is in its life cycle.</summary>
public enum FallbackPositionQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a fallback position quota and its current status.</summary>
public sealed class FallbackPositionQuota(string code, string title, double hours, string name, int windowDays, string region, string department, double rating, bool visible, int endsAt, FallbackPositionQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Hours { get; set; } = hours;
    public string Name { get; set; } = name;
    public int WindowDays { get; set; } = windowDays;
    public string Region { get; set; } = region;
    public string Department { get; set; } = department;
    public double Rating { get; set; } = rating;
    public bool Visible { get; set; } = visible;
    public int EndsAt { get; set; } = endsAt;
    public FallbackPositionQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a fallback position quota in status open.</summary>
    public static FallbackPositionQuota NewFallbackPositionQuota(string code, string title)
    {
        return new FallbackPositionQuota(
            code,
            title,
            0.0,
            "",
            0,
            "",
            "",
            0.0,
            false,
            0,
            FallbackPositionQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPositionQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPositionQuotaStatus next)
    {
        switch (Status)
        {
            case FallbackPositionQuotaStatus.Open:
                return next == FallbackPositionQuotaStatus.InReview || next == FallbackPositionQuotaStatus.Archived;
            case FallbackPositionQuotaStatus.InReview:
                return next == FallbackPositionQuotaStatus.Reopened || next == FallbackPositionQuotaStatus.Archived;
            case FallbackPositionQuotaStatus.Resolved:
                return next == FallbackPositionQuotaStatus.Archived;
            case FallbackPositionQuotaStatus.Reopened:
                return next == FallbackPositionQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(FallbackPositionQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback position quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"fallback position quota"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Hours - (double)EndsAt, 2);
    }

    public FallbackPositionQuota Copy()
    {
        return new FallbackPositionQuota(
            Code,
            Title,
            Hours,
            Name,
            WindowDays,
            Region,
            Department,
            Rating,
            Visible,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
