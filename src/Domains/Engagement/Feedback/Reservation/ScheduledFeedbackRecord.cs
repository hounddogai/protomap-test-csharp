namespace Acme.Workforce.Engagement.Feedback.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled feedback record is in its life cycle.</summary>
public enum ScheduledFeedbackRecordStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One scheduled feedback record, with what the planners need to act on it.</summary>
public sealed class ScheduledFeedbackRecord(string code, string label, int retries, double cost, double hours, string segment, ScheduledFeedbackRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Retries { get; set; } = retries;
    public double Cost { get; set; } = cost;
    public double Hours { get; set; } = hours;
    public string Segment { get; set; } = segment;
    public ScheduledFeedbackRecordStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled feedback record in status proposed.</summary>
    public static ScheduledFeedbackRecord NewScheduledFeedbackRecord(string code, string label)
    {
        return new ScheduledFeedbackRecord(code, label, 0, 0.0, 0.0, "", ScheduledFeedbackRecordStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledFeedbackRecordStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledFeedbackRecordStatus next)
    {
        switch (Status)
        {
            case ScheduledFeedbackRecordStatus.Proposed:
                return next == ScheduledFeedbackRecordStatus.Approved || next == ScheduledFeedbackRecordStatus.Rejected;
            case ScheduledFeedbackRecordStatus.Approved:
                return next == ScheduledFeedbackRecordStatus.Rejected || next == ScheduledFeedbackRecordStatus.Withdrawn;
            case ScheduledFeedbackRecordStatus.Rejected:
                switch (next)
                {
                    case ScheduledFeedbackRecordStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledFeedbackRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled feedback record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"scheduled feedback record"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Cost - (double)Retries, 2);
    }

    public ScheduledFeedbackRecord Copy()
    {
        return new ScheduledFeedbackRecord(Code, Label, Retries, Cost, Hours, Segment, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
