namespace Acme.Workforce.Engagement.Feedback.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback record is in its life cycle.</summary>
public enum FeedbackRecordStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a feedback record and its current status.</summary>
public sealed class FeedbackRecord(string code, string label, string description, double score, string team, int leaveDays, FeedbackRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public string Team { get; set; } = team;
    public int LeaveDays { get; set; } = leaveDays;
    public FeedbackRecordStatus Status { get; set; } = status;

    /// <summary>Starts a feedback record in status new.</summary>
    public static FeedbackRecord NewFeedbackRecord(string code, string label)
    {
        return new FeedbackRecord(code, label, "", 0.0, "", 0, FeedbackRecordStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackRecordStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackRecordStatus next)
    {
        switch (Status)
        {
            case FeedbackRecordStatus.New:
                return next == FeedbackRecordStatus.Queued || next == FeedbackRecordStatus.Failed;
            case FeedbackRecordStatus.Queued:
                switch (next)
                {
                    case FeedbackRecordStatus.Running:
                        return true;
                }
                return false;
            case FeedbackRecordStatus.Running:
                return next == FeedbackRecordStatus.Failed;
            case FeedbackRecordStatus.Done:
                switch (next)
                {
                    case FeedbackRecordStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"feedback record"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round(Score - (double)LeaveDays, 2);
    }

    public FeedbackRecord Copy()
    {
        return new FeedbackRecord(Code, Label, Description, Score, Team, LeaveDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
