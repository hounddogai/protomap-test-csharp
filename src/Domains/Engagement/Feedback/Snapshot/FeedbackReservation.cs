namespace Acme.Workforce.Engagement.Feedback.Snapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback reservation is in its life cycle.</summary>
public enum FeedbackReservationStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One feedback reservation, with what the planners need to act on it.</summary>
public sealed class FeedbackReservation(string code, string label, string salaryBand, string reviewer, int endsAt, double maximum, int priority, FeedbackReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Reviewer { get; set; } = reviewer;
    public int EndsAt { get; set; } = endsAt;
    public double Maximum { get; set; } = maximum;
    public int Priority { get; set; } = priority;
    public FeedbackReservationStatus Status { get; set; } = status;

    /// <summary>Starts a feedback reservation in status requested.</summary>
    public static FeedbackReservation NewFeedbackReservation(string code, string label)
    {
        return new FeedbackReservation(code, label, "", "", 0, 0.0, 0, FeedbackReservationStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackReservationStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackReservationStatus next)
    {
        switch (Status)
        {
            case FeedbackReservationStatus.Requested:
                return next == FeedbackReservationStatus.Expired;
            case FeedbackReservationStatus.Granted:
                return next == FeedbackReservationStatus.Expired;
            case FeedbackReservationStatus.Revoked:
                switch (next)
                {
                    case FeedbackReservationStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Priority * Maximum, 2);
    }

    public FeedbackReservation Copy()
    {
        return new FeedbackReservation(Code, Label, SalaryBand, Reviewer, EndsAt, Maximum, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
