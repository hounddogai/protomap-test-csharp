namespace Acme.Workforce.Engagement.Feedback.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary feedback claim is in its life cycle.</summary>
public enum PrimaryFeedbackClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A primary feedback claim. Its code is unique within a tenant.</summary>
public sealed class PrimaryFeedbackClaim(string code, string title, string summary, int tenureMonths, int priority, int endsAt, int batchSize, string segment, int sequence, double cost, PrimaryFeedbackClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Priority { get; set; } = priority;
    public int EndsAt { get; set; } = endsAt;
    public int BatchSize { get; set; } = batchSize;
    public string Segment { get; set; } = segment;
    public int Sequence { get; set; } = sequence;
    public double Cost { get; set; } = cost;
    public PrimaryFeedbackClaimStatus Status { get; set; } = status;

    /// <summary>Starts a primary feedback claim in status idle.</summary>
    public static PrimaryFeedbackClaim NewPrimaryFeedbackClaim(string code, string title)
    {
        return new PrimaryFeedbackClaim(code, title, "", 0, 0, 0, 0, "", 0, 0.0, PrimaryFeedbackClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryFeedbackClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryFeedbackClaimStatus next)
    {
        switch (Status)
        {
            case PrimaryFeedbackClaimStatus.Idle:
                return next == PrimaryFeedbackClaimStatus.Ready || next == PrimaryFeedbackClaimStatus.Draining;
            case PrimaryFeedbackClaimStatus.Warming:
                return next == PrimaryFeedbackClaimStatus.Ready || next == PrimaryFeedbackClaimStatus.Draining;
            case PrimaryFeedbackClaimStatus.Ready:
                switch (next)
                {
                    case PrimaryFeedbackClaimStatus.Draining:
                    case PrimaryFeedbackClaimStatus.Stopped:
                        return true;
                }
                return false;
            case PrimaryFeedbackClaimStatus.Draining:
                return next == PrimaryFeedbackClaimStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PrimaryFeedbackClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary feedback claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)BatchSize - (double)EndsAt, 2);
    }

    public PrimaryFeedbackClaim Copy()
    {
        return new PrimaryFeedbackClaim(
            Code,
            Title,
            Summary,
            TenureMonths,
            Priority,
            EndsAt,
            BatchSize,
            Segment,
            Sequence,
            Cost,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
