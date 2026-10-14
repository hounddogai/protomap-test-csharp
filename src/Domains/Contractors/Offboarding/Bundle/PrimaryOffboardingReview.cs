namespace Acme.Workforce.Contractors.Offboarding.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary offboarding review is in its life cycle.</summary>
public enum PrimaryOffboardingReviewStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary offboarding review as the offboarding team keeps it.</summary>
public sealed class PrimaryOffboardingReview(string code, string label, int endsAt, string segment, double budget, int capacity, int durationMinutes, double floorValue, double minimum, double ceiling, PrimaryOffboardingReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int EndsAt { get; set; } = endsAt;
    public string Segment { get; set; } = segment;
    public double Budget { get; set; } = budget;
    public int Capacity { get; set; } = capacity;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double FloorValue { get; set; } = floorValue;
    public double Minimum { get; set; } = minimum;
    public double Ceiling { get; set; } = ceiling;
    public PrimaryOffboardingReviewStatus Status { get; set; } = status;

    /// <summary>Starts a primary offboarding review in status idle.</summary>
    public static PrimaryOffboardingReview NewPrimaryOffboardingReview(string code, string label)
    {
        return new PrimaryOffboardingReview(
            code,
            label,
            0,
            "",
            0.0,
            0,
            0,
            0.0,
            0.0,
            0.0,
            PrimaryOffboardingReviewStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryOffboardingReviewStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryOffboardingReviewStatus next)
    {
        switch (Status)
        {
            case PrimaryOffboardingReviewStatus.Idle:
                switch (next)
                {
                    case PrimaryOffboardingReviewStatus.Warming:
                    case PrimaryOffboardingReviewStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryOffboardingReviewStatus.Warming:
                return next == PrimaryOffboardingReviewStatus.Ready || next == PrimaryOffboardingReviewStatus.Stopped;
            case PrimaryOffboardingReviewStatus.Ready:
                return next == PrimaryOffboardingReviewStatus.Stopped;
            case PrimaryOffboardingReviewStatus.Draining:
                switch (next)
                {
                    case PrimaryOffboardingReviewStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryOffboardingReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary offboarding review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedTotal()
    {
        return Math.Round(FloorValue - Budget, 2);
    }

    public PrimaryOffboardingReview Copy()
    {
        return new PrimaryOffboardingReview(
            Code,
            Label,
            EndsAt,
            Segment,
            Budget,
            Capacity,
            DurationMinutes,
            FloorValue,
            Minimum,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
