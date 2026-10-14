namespace Acme.Workforce.Assets.Refresh.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling refresh preference is in its life cycle.</summary>
public enum RollingRefreshPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a rolling refresh preference and its current status.</summary>
public sealed class RollingRefreshPreference(string code, string label, string currency, int durationMinutes, string badge, List<string> tags, string country, bool active, double cost, string externalRef, RollingRefreshPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Badge { get; set; } = badge;
    public List<string> Tags { get; set; } = tags;
    public string Country { get; set; } = country;
    public bool Active { get; set; } = active;
    public double Cost { get; set; } = cost;
    public string ExternalRef { get; set; } = externalRef;
    public RollingRefreshPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a rolling refresh preference in status open.</summary>
    public static RollingRefreshPreference NewRollingRefreshPreference(string code, string label)
    {
        return new RollingRefreshPreference(
            code,
            label,
            "",
            0,
            "",
            new List<string>(),
            "",
            false,
            0.0,
            "",
            RollingRefreshPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRefreshPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRefreshPreferenceStatus next)
    {
        switch (Status)
        {
            case RollingRefreshPreferenceStatus.Open:
                return next == RollingRefreshPreferenceStatus.Reopened || next == RollingRefreshPreferenceStatus.Archived;
            case RollingRefreshPreferenceStatus.InReview:
                return next == RollingRefreshPreferenceStatus.Resolved || next == RollingRefreshPreferenceStatus.Reopened;
            case RollingRefreshPreferenceStatus.Resolved:
                switch (next)
                {
                    case RollingRefreshPreferenceStatus.Reopened:
                        return true;
                }
                return false;
            case RollingRefreshPreferenceStatus.Reopened:
                return next == RollingRefreshPreferenceStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RollingRefreshPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling refresh preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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
        return Math.Round((double)DurationMinutes * Cost, 2);
    }

    public RollingRefreshPreference Copy()
    {
        return new RollingRefreshPreference(
            Code,
            Label,
            Currency,
            DurationMinutes,
            Badge,
            Tags,
            Country,
            Active,
            Cost,
            ExternalRef,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
