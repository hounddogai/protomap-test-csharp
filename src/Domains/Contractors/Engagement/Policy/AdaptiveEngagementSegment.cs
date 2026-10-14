namespace Acme.Workforce.Contractors.Engagement.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive engagement segment is in its life cycle.</summary>
public enum AdaptiveEngagementSegmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a adaptive engagement segment and its current status.</summary>
public sealed class AdaptiveEngagementSegment(string code, string label, int endsAt, string title, string unit, double maximum, string jobCode, AdaptiveEngagementSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public double Maximum { get; set; } = maximum;
    public string JobCode { get; set; } = jobCode;
    public AdaptiveEngagementSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive engagement segment in status requested.</summary>
    public static AdaptiveEngagementSegment NewAdaptiveEngagementSegment(string code, string label)
    {
        return new AdaptiveEngagementSegment(
            code,
            label,
            0,
            "",
            "",
            0.0,
            "",
            AdaptiveEngagementSegmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveEngagementSegmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveEngagementSegmentStatus next)
    {
        switch (Status)
        {
            case AdaptiveEngagementSegmentStatus.Requested:
                switch (next)
                {
                    case AdaptiveEngagementSegmentStatus.Granted:
                        return true;
                }
                return false;
            case AdaptiveEngagementSegmentStatus.Granted:
                switch (next)
                {
                    case AdaptiveEngagementSegmentStatus.Revoked:
                        return true;
                }
                return false;
            case AdaptiveEngagementSegmentStatus.Revoked:
                switch (next)
                {
                    case AdaptiveEngagementSegmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveEngagementSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive engagement segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round((double)EndsAt + Maximum, 2);
    }

    public AdaptiveEngagementSegment Copy()
    {
        return new AdaptiveEngagementSegment(Code, Label, EndsAt, Title, Unit, Maximum, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
