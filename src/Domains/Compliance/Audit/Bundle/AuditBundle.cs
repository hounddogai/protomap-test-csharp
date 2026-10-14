namespace Acme.Workforce.Compliance.Audit.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit bundle is in its life cycle.</summary>
public enum AuditBundleStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a audit bundle and its current status.</summary>
public sealed class AuditBundle(string code, string title, int endsAt, double ratio, int durationMinutes, int startsAt, int limit, string region, string badge, Dictionary<string, string> attributes, int capacity, AuditBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public double Ratio { get; set; } = ratio;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int StartsAt { get; set; } = startsAt;
    public int Limit { get; set; } = limit;
    public string Region { get; set; } = region;
    public string Badge { get; set; } = badge;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Capacity { get; set; } = capacity;
    public AuditBundleStatus Status { get; set; } = status;

    /// <summary>Starts a audit bundle in status open.</summary>
    public static AuditBundle NewAuditBundle(string code, string title)
    {
        return new AuditBundle(
            code,
            title,
            0,
            0.0,
            0,
            0,
            0,
            "",
            "",
            new Dictionary<string, string>(),
            0,
            AuditBundleStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditBundleStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditBundleStatus next)
    {
        switch (Status)
        {
            case AuditBundleStatus.Open:
                switch (next)
                {
                    case AuditBundleStatus.Archived:
                        return true;
                }
                return false;
            case AuditBundleStatus.InReview:
                switch (next)
                {
                    case AuditBundleStatus.Archived:
                        return true;
                }
                return false;
            case AuditBundleStatus.Resolved:
                return next == AuditBundleStatus.Reopened;
            case AuditBundleStatus.Reopened:
                switch (next)
                {
                    case AuditBundleStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AuditBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Capacity + Ratio, 2);
    }

    public AuditBundle Copy()
    {
        return new AuditBundle(
            Code,
            Title,
            EndsAt,
            Ratio,
            DurationMinutes,
            StartsAt,
            Limit,
            Region,
            Badge,
            Attributes,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
