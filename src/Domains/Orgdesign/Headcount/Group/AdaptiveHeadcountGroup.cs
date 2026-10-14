namespace Acme.Workforce.Orgdesign.Headcount.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive headcount group is in its life cycle.</summary>
public enum AdaptiveHeadcountGroupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A adaptive headcount group as the headcount team keeps it.</summary>
public sealed class AdaptiveHeadcountGroup(string code, string title, string locale, int expiresAt, int windowDays, string managerId, string label, string segment, AdaptiveHeadcountGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public int ExpiresAt { get; set; } = expiresAt;
    public int WindowDays { get; set; } = windowDays;
    public string ManagerId { get; set; } = managerId;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public AdaptiveHeadcountGroupStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive headcount group in status open.</summary>
    public static AdaptiveHeadcountGroup NewAdaptiveHeadcountGroup(string code, string title)
    {
        return new AdaptiveHeadcountGroup(code, title, "", 0, 0, "", "", "", AdaptiveHeadcountGroupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveHeadcountGroupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveHeadcountGroupStatus next)
    {
        switch (Status)
        {
            case AdaptiveHeadcountGroupStatus.Open:
                switch (next)
                {
                    case AdaptiveHeadcountGroupStatus.Reopened:
                    case AdaptiveHeadcountGroupStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveHeadcountGroupStatus.InReview:
                switch (next)
                {
                    case AdaptiveHeadcountGroupStatus.Resolved:
                    case AdaptiveHeadcountGroupStatus.Archived:
                        return true;
                }
                return false;
            case AdaptiveHeadcountGroupStatus.Resolved:
                return next == AdaptiveHeadcountGroupStatus.Archived;
            case AdaptiveHeadcountGroupStatus.Reopened:
                switch (next)
                {
                    case AdaptiveHeadcountGroupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveHeadcountGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive headcount group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveLoad()
    {
        return Math.Round((double)WindowDays + (double)ExpiresAt, 2);
    }

    public AdaptiveHeadcountGroup Copy()
    {
        return new AdaptiveHeadcountGroup(
            Code,
            Title,
            Locale,
            ExpiresAt,
            WindowDays,
            ManagerId,
            Label,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
