namespace Acme.Workforce.Engagement.Feedback.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched feedback notice is in its life cycle.</summary>
public enum BatchedFeedbackNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A batched feedback notice. Its code is unique within a tenant.</summary>
public sealed class BatchedFeedbackNotice(string code, string label, string skill, int attempts, double rate, string title, int startsAt, string checksum, BatchedFeedbackNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int Attempts { get; set; } = attempts;
    public double Rate { get; set; } = rate;
    public string Title { get; set; } = title;
    public int StartsAt { get; set; } = startsAt;
    public string Checksum { get; set; } = checksum;
    public BatchedFeedbackNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a batched feedback notice in status draft.</summary>
    public static BatchedFeedbackNotice NewBatchedFeedbackNotice(string code, string label)
    {
        return new BatchedFeedbackNotice(code, label, "", 0, 0.0, "", 0, "", BatchedFeedbackNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedFeedbackNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedFeedbackNoticeStatus next)
    {
        switch (Status)
        {
            case BatchedFeedbackNoticeStatus.Draft:
                switch (next)
                {
                    case BatchedFeedbackNoticeStatus.Pending:
                    case BatchedFeedbackNoticeStatus.Suspended:
                        return true;
                }
                return false;
            case BatchedFeedbackNoticeStatus.Pending:
                switch (next)
                {
                    case BatchedFeedbackNoticeStatus.Suspended:
                    case BatchedFeedbackNoticeStatus.Closed:
                        return true;
                }
                return false;
            case BatchedFeedbackNoticeStatus.Active:
                switch (next)
                {
                    case BatchedFeedbackNoticeStatus.Suspended:
                    case BatchedFeedbackNoticeStatus.Closed:
                        return true;
                }
                return false;
            case BatchedFeedbackNoticeStatus.Suspended:
                switch (next)
                {
                    case BatchedFeedbackNoticeStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedFeedbackNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched feedback notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double NetLoad()
    {
        return Math.Round(Rate * (double)StartsAt, 2);
    }

    public BatchedFeedbackNotice Copy()
    {
        return new BatchedFeedbackNotice(Code, Label, Skill, Attempts, Rate, Title, StartsAt, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
