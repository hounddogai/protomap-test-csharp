namespace Acme.Workforce.Engagement.PulseSurvey.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback pulse survey segment is in its life cycle.</summary>
public enum FallbackPulseSurveySegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One fallback pulse survey segment, with what the planners need to act on it.</summary>
public sealed class FallbackPulseSurveySegment(string code, string name, string grade, int durationMinutes, double rating, FallbackPulseSurveySegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Grade { get; set; } = grade;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Rating { get; set; } = rating;
    public FallbackPulseSurveySegmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback pulse survey segment in status idle.</summary>
    public static FallbackPulseSurveySegment NewFallbackPulseSurveySegment(string code, string name)
    {
        return new FallbackPulseSurveySegment(code, name, "", 0, 0.0, FallbackPulseSurveySegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPulseSurveySegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPulseSurveySegmentStatus next)
    {
        switch (Status)
        {
            case FallbackPulseSurveySegmentStatus.Idle:
                switch (next)
                {
                    case FallbackPulseSurveySegmentStatus.Ready:
                        return true;
                }
                return false;
            case FallbackPulseSurveySegmentStatus.Warming:
                switch (next)
                {
                    case FallbackPulseSurveySegmentStatus.Ready:
                    case FallbackPulseSurveySegmentStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackPulseSurveySegmentStatus.Ready:
                switch (next)
                {
                    case FallbackPulseSurveySegmentStatus.Draining:
                    case FallbackPulseSurveySegmentStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackPulseSurveySegmentStatus.Draining:
                switch (next)
                {
                    case FallbackPulseSurveySegmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPulseSurveySegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback pulse survey segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"fallback pulse survey segment"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)DurationMinutes + Rating, 2);
    }

    public FallbackPulseSurveySegment Copy()
    {
        return new FallbackPulseSurveySegment(Code, Name, Grade, DurationMinutes, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
