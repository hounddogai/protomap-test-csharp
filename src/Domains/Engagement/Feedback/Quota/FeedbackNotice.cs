namespace Acme.Workforce.Engagement.Feedback.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback notice is in its life cycle.</summary>
public enum FeedbackNoticeStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A feedback notice as the feedback team keeps it.</summary>
public sealed class FeedbackNotice(string code, string label, int revision, bool visible, int updatedAt, double minimum, int tenureMonths, double floorValue, double ceiling, FeedbackNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public bool Visible { get; set; } = visible;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Minimum { get; set; } = minimum;
    public int TenureMonths { get; set; } = tenureMonths;
    public double FloorValue { get; set; } = floorValue;
    public double Ceiling { get; set; } = ceiling;
    public FeedbackNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a feedback notice in status draft.</summary>
    public static FeedbackNotice NewFeedbackNotice(string code, string label)
    {
        return new FeedbackNotice(code, label, 0, false, 0, 0.0, 0, 0.0, 0.0, FeedbackNoticeStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackNoticeStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackNoticeStatus next)
    {
        switch (Status)
        {
            case FeedbackNoticeStatus.Draft:
                return next == FeedbackNoticeStatus.Pending;
            case FeedbackNoticeStatus.Pending:
                return next == FeedbackNoticeStatus.Suspended;
            case FeedbackNoticeStatus.Active:
                switch (next)
                {
                    case FeedbackNoticeStatus.Suspended:
                    case FeedbackNoticeStatus.Closed:
                        return true;
                }
                return false;
            case FeedbackNoticeStatus.Suspended:
                return next == FeedbackNoticeStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FeedbackNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedValue()
    {
        return Math.Round((double)TenureMonths - Ceiling, 2);
    }

    public FeedbackNotice Copy()
    {
        return new FeedbackNotice(
            Code,
            Label,
            Revision,
            Visible,
            UpdatedAt,
            Minimum,
            TenureMonths,
            FloorValue,
            Ceiling,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
