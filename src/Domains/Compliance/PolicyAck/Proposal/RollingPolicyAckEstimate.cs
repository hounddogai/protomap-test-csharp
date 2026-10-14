namespace Acme.Workforce.Compliance.PolicyAck.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling policy ack estimate is in its life cycle.</summary>
public enum RollingPolicyAckEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One rolling policy ack estimate, with what the planners need to act on it.</summary>
public sealed class RollingPolicyAckEstimate(string code, string title, int batchSize, double budget, double rate, bool visible, int durationMinutes, RollingPolicyAckEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public double Budget { get; set; } = budget;
    public double Rate { get; set; } = rate;
    public bool Visible { get; set; } = visible;
    public int DurationMinutes { get; set; } = durationMinutes;
    public RollingPolicyAckEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a rolling policy ack estimate in status open.</summary>
    public static RollingPolicyAckEstimate NewRollingPolicyAckEstimate(string code, string title)
    {
        return new RollingPolicyAckEstimate(code, title, 0, 0.0, 0.0, false, 0, RollingPolicyAckEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingPolicyAckEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingPolicyAckEstimateStatus next)
    {
        switch (Status)
        {
            case RollingPolicyAckEstimateStatus.Open:
                switch (next)
                {
                    case RollingPolicyAckEstimateStatus.InReview:
                    case RollingPolicyAckEstimateStatus.Resolved:
                        return true;
                }
                return false;
            case RollingPolicyAckEstimateStatus.InReview:
                return next == RollingPolicyAckEstimateStatus.Reopened || next == RollingPolicyAckEstimateStatus.Archived;
            case RollingPolicyAckEstimateStatus.Resolved:
                return next == RollingPolicyAckEstimateStatus.Reopened || next == RollingPolicyAckEstimateStatus.Archived;
            case RollingPolicyAckEstimateStatus.Reopened:
                switch (next)
                {
                    case RollingPolicyAckEstimateStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingPolicyAckEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling policy ack estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)DurationMinutes * (double)BatchSize, 2);
    }

    public RollingPolicyAckEstimate Copy()
    {
        return new RollingPolicyAckEstimate(Code, Title, BatchSize, Budget, Rate, Visible, DurationMinutes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
