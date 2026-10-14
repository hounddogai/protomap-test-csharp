namespace Acme.Workforce.Engagement.PulseSurvey.SurveyQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling pulse survey segment is in its life cycle.</summary>
public enum RollingPulseSurveySegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A rolling pulse survey segment as the pulse survey team keeps it.</summary>
public sealed class RollingPulseSurveySegment(string code, string label, int windowDays, string location, double rating, double minimum, string segment, double rate, RollingPulseSurveySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public double Rating { get; set; } = rating;
    public double Minimum { get; set; } = minimum;
    public string Segment { get; set; } = segment;
    public double Rate { get; set; } = rate;
    public RollingPulseSurveySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a rolling pulse survey segment in status planned.</summary>
    public static RollingPulseSurveySegment NewRollingPulseSurveySegment(string code, string label)
    {
        return new RollingPulseSurveySegment(
            code,
            label,
            0,
            "",
            0.0,
            0.0,
            "",
            0.0,
            RollingPulseSurveySegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPulseSurveySegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPulseSurveySegmentStatus next)
    {
        switch (Status)
        {
            case RollingPulseSurveySegmentStatus.Planned:
                return next == RollingPulseSurveySegmentStatus.Cancelled;
            case RollingPulseSurveySegmentStatus.Scheduled:
                return next == RollingPulseSurveySegmentStatus.Completed;
            case RollingPulseSurveySegmentStatus.InProgress:
                return next == RollingPulseSurveySegmentStatus.Completed;
            case RollingPulseSurveySegmentStatus.Completed:
                switch (next)
                {
                    case RollingPulseSurveySegmentStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPulseSurveySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling pulse survey segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"rolling pulse survey segment"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round(Minimum - Rating, 2);
    }

    public RollingPulseSurveySegment Copy()
    {
        return new RollingPulseSurveySegment(
            Code,
            Label,
            WindowDays,
            Location,
            Rating,
            Minimum,
            Segment,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
