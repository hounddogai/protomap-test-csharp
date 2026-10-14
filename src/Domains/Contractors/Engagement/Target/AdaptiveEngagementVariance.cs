namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive engagement variance is in its life cycle.</summary>
public enum AdaptiveEngagementVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One adaptive engagement variance, with what the planners need to act on it.</summary>
public sealed class AdaptiveEngagementVariance(string code, string label, string badge, string segment, string source, int leaveDays, AdaptiveEngagementVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Badge { get; set; } = badge;
    public string Segment { get; set; } = segment;
    public string Source { get; set; } = source;
    public int LeaveDays { get; set; } = leaveDays;
    public AdaptiveEngagementVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive engagement variance in status created.</summary>
    public static AdaptiveEngagementVariance NewAdaptiveEngagementVariance(string code, string label)
    {
        return new AdaptiveEngagementVariance(code, label, "", "", "", 0, AdaptiveEngagementVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveEngagementVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveEngagementVarianceStatus next)
    {
        switch (Status)
        {
            case AdaptiveEngagementVarianceStatus.Created:
                return next == AdaptiveEngagementVarianceStatus.Published || next == AdaptiveEngagementVarianceStatus.Retired;
            case AdaptiveEngagementVarianceStatus.Validated:
                switch (next)
                {
                    case AdaptiveEngagementVarianceStatus.Retired:
                        return true;
                }
                return false;
            case AdaptiveEngagementVarianceStatus.Published:
                return next == AdaptiveEngagementVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(AdaptiveEngagementVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive engagement variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"adaptive engagement variance"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)LeaveDays;
    }

    public AdaptiveEngagementVariance Copy()
    {
        return new AdaptiveEngagementVariance(Code, Label, Badge, Segment, Source, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
