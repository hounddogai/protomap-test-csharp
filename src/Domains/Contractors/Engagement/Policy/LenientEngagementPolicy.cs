namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient engagement policy is in its life cycle.</summary>
public enum LenientEngagementPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a lenient engagement policy and its current status.</summary>
public sealed class LenientEngagementPolicy(string code, string title, double ratio, string reviewer, Dictionary<string, string> attributes, int startsAt, int leaveDays, string locale, bool approved, int graceDays, LenientEngagementPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Reviewer { get; set; } = reviewer;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public int LeaveDays { get; set; } = leaveDays;
    public string Locale { get; set; } = locale;
    public bool Approved { get; set; } = approved;
    public int GraceDays { get; set; } = graceDays;
    public LenientEngagementPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a lenient engagement policy in status planned.</summary>
    public static LenientEngagementPolicy NewLenientEngagementPolicy(string code, string title)
    {
        return new LenientEngagementPolicy(
            code,
            title,
            0.0,
            "",
            new Dictionary<string, string>(),
            0,
            0,
            "",
            false,
            0,
            LenientEngagementPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientEngagementPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientEngagementPolicyStatus next)
    {
        switch (Status)
        {
            case LenientEngagementPolicyStatus.Planned:
                switch (next)
                {
                    case LenientEngagementPolicyStatus.Completed:
                    case LenientEngagementPolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case LenientEngagementPolicyStatus.Scheduled:
                switch (next)
                {
                    case LenientEngagementPolicyStatus.Completed:
                        return true;
                }
                return false;
            case LenientEngagementPolicyStatus.InProgress:
                return next == LenientEngagementPolicyStatus.Completed || next == LenientEngagementPolicyStatus.Cancelled;
            case LenientEngagementPolicyStatus.Completed:
                return next == LenientEngagementPolicyStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(LenientEngagementPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient engagement policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double ProjectedExposure()
    {
        return Math.Round(Ratio + (double)LeaveDays, 2);
    }

    public LenientEngagementPolicy Copy()
    {
        return new LenientEngagementPolicy(
            Code,
            Title,
            Ratio,
            Reviewer,
            Attributes,
            StartsAt,
            LeaveDays,
            Locale,
            Approved,
            GraceDays,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
