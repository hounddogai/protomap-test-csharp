namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback career path adjustment is in its life cycle.</summary>
public enum FallbackCareerPathAdjustmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a fallback career path adjustment and its current status.</summary>
public sealed class FallbackCareerPathAdjustment(string code, string label, string managerId, double floorValue, int endsAt, double minimum, string badge, FallbackCareerPathAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ManagerId { get; set; } = managerId;
    public double FloorValue { get; set; } = floorValue;
    public int EndsAt { get; set; } = endsAt;
    public double Minimum { get; set; } = minimum;
    public string Badge { get; set; } = badge;
    public FallbackCareerPathAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a fallback career path adjustment in status idle.</summary>
    public static FallbackCareerPathAdjustment NewFallbackCareerPathAdjustment(string code, string label)
    {
        return new FallbackCareerPathAdjustment(
            code,
            label,
            "",
            0.0,
            0,
            0.0,
            "",
            FallbackCareerPathAdjustmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackCareerPathAdjustmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackCareerPathAdjustmentStatus next)
    {
        switch (Status)
        {
            case FallbackCareerPathAdjustmentStatus.Idle:
                switch (next)
                {
                    case FallbackCareerPathAdjustmentStatus.Draining:
                        return true;
                }
                return false;
            case FallbackCareerPathAdjustmentStatus.Warming:
                switch (next)
                {
                    case FallbackCareerPathAdjustmentStatus.Draining:
                    case FallbackCareerPathAdjustmentStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackCareerPathAdjustmentStatus.Ready:
                return next == FallbackCareerPathAdjustmentStatus.Draining;
            case FallbackCareerPathAdjustmentStatus.Draining:
                return next == FallbackCareerPathAdjustmentStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(FallbackCareerPathAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback career path adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Minimum * FloorValue, 2);
    }

    public FallbackCareerPathAdjustment Copy()
    {
        return new FallbackCareerPathAdjustment(Code, Label, ManagerId, FloorValue, EndsAt, Minimum, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
