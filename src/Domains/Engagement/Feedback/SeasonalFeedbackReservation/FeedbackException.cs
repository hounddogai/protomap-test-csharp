namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback exception is in its life cycle.</summary>
public enum FeedbackExceptionStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a feedback exception and its current status.</summary>
public sealed class FeedbackException(string code, string label, int durationMinutes, int updatedAt, int startsAt, double fte, string unit, string summary, string team, int limit, FeedbackExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int UpdatedAt { get; set; } = updatedAt;
    public int StartsAt { get; set; } = startsAt;
    public double Fte { get; set; } = fte;
    public string Unit { get; set; } = unit;
    public string Summary { get; set; } = summary;
    public string Team { get; set; } = team;
    public int Limit { get; set; } = limit;
    public FeedbackExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a feedback exception in status requested.</summary>
    public static FeedbackException NewFeedbackException(string code, string label)
    {
        return new FeedbackException(code, label, 0, 0, 0, 0.0, "", "", "", 0, FeedbackExceptionStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackExceptionStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackExceptionStatus next)
    {
        switch (Status)
        {
            case FeedbackExceptionStatus.Requested:
                switch (next)
                {
                    case FeedbackExceptionStatus.Granted:
                    case FeedbackExceptionStatus.Revoked:
                        return true;
                }
                return false;
            case FeedbackExceptionStatus.Granted:
                return next == FeedbackExceptionStatus.Revoked || next == FeedbackExceptionStatus.Expired;
            case FeedbackExceptionStatus.Revoked:
                switch (next)
                {
                    case FeedbackExceptionStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"feedback exception"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Fte * (double)Limit, 2);
    }

    public FeedbackException Copy()
    {
        return new FeedbackException(
            Code,
            Label,
            DurationMinutes,
            UpdatedAt,
            StartsAt,
            Fte,
            Unit,
            Summary,
            Team,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
