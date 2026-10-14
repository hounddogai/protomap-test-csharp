namespace Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict feedback reservation is in its life cycle.</summary>
public enum StrictFeedbackReservationStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a strict feedback reservation and its current status.</summary>
public sealed class StrictFeedbackReservation(string code, string label, double margin, int leaveDays, string managerId, string timezone, StrictFeedbackReservationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Margin { get; set; } = margin;
    public int LeaveDays { get; set; } = leaveDays;
    public string ManagerId { get; set; } = managerId;
    public string Timezone { get; set; } = timezone;
    public StrictFeedbackReservationStatus Status { get; set; } = status;

    /// <summary>Starts a strict feedback reservation in status open.</summary>
    public static StrictFeedbackReservation NewStrictFeedbackReservation(string code, string label)
    {
        return new StrictFeedbackReservation(code, label, 0.0, 0, "", "", StrictFeedbackReservationStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictFeedbackReservationStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictFeedbackReservationStatus next)
    {
        switch (Status)
        {
            case StrictFeedbackReservationStatus.Open:
                return next == StrictFeedbackReservationStatus.InReview || next == StrictFeedbackReservationStatus.Archived;
            case StrictFeedbackReservationStatus.InReview:
                switch (next)
                {
                    case StrictFeedbackReservationStatus.Resolved:
                    case StrictFeedbackReservationStatus.Archived:
                        return true;
                }
                return false;
            case StrictFeedbackReservationStatus.Resolved:
                return next == StrictFeedbackReservationStatus.Archived;
            case StrictFeedbackReservationStatus.Reopened:
                switch (next)
                {
                    case StrictFeedbackReservationStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictFeedbackReservationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict feedback reservation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"strict feedback reservation"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Margin + (double)LeaveDays, 2);
    }

    public StrictFeedbackReservation Copy()
    {
        return new StrictFeedbackReservation(Code, Label, Margin, LeaveDays, ManagerId, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
