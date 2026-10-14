namespace Acme.Workforce.Compliance.TrainingDue.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback training due segment is in its life cycle.</summary>
public enum FallbackTrainingDueSegmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A fallback training due segment. Its code is unique within a tenant.</summary>
public sealed class FallbackTrainingDueSegment(string code, string label, int endsAt, string title, string costCenter, double minimum, int limit, int expiresAt, int graceDays, int leaveDays, string location, FallbackTrainingDueSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Title { get; set; } = title;
    public string CostCenter { get; set; } = costCenter;
    public double Minimum { get; set; } = minimum;
    public int Limit { get; set; } = limit;
    public int ExpiresAt { get; set; } = expiresAt;
    public int GraceDays { get; set; } = graceDays;
    public int LeaveDays { get; set; } = leaveDays;
    public string Location { get; set; } = location;
    public FallbackTrainingDueSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback training due segment in status idle.</summary>
    public static FallbackTrainingDueSegment NewFallbackTrainingDueSegment(string code, string label)
    {
        return new FallbackTrainingDueSegment(
            code,
            label,
            0,
            "",
            "",
            0.0,
            0,
            0,
            0,
            0,
            "",
            FallbackTrainingDueSegmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackTrainingDueSegmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackTrainingDueSegmentStatus next)
    {
        switch (Status)
        {
            case FallbackTrainingDueSegmentStatus.Idle:
                return next == FallbackTrainingDueSegmentStatus.Draining;
            case FallbackTrainingDueSegmentStatus.Warming:
                return next == FallbackTrainingDueSegmentStatus.Stopped;
            case FallbackTrainingDueSegmentStatus.Ready:
                return next == FallbackTrainingDueSegmentStatus.Draining;
            case FallbackTrainingDueSegmentStatus.Draining:
                switch (next)
                {
                    case FallbackTrainingDueSegmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackTrainingDueSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback training due segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"fallback training due segment"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Minimum - (double)ExpiresAt, 2);
    }

    public FallbackTrainingDueSegment Copy()
    {
        return new FallbackTrainingDueSegment(
            Code,
            Label,
            EndsAt,
            Title,
            CostCenter,
            Minimum,
            Limit,
            ExpiresAt,
            GraceDays,
            LeaveDays,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
