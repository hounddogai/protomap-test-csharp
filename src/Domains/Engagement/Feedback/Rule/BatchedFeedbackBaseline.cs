namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched feedback baseline is in its life cycle.</summary>
public enum BatchedFeedbackBaselineStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One batched feedback baseline, with what the planners need to act on it.</summary>
public sealed class BatchedFeedbackBaseline(string code, string title, string segment, int graceDays, string department, int endsAt, string currency, string employeeId, string jobCode, BatchedFeedbackBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public int GraceDays { get; set; } = graceDays;
    public string Department { get; set; } = department;
    public int EndsAt { get; set; } = endsAt;
    public string Currency { get; set; } = currency;
    public string EmployeeId { get; set; } = employeeId;
    public string JobCode { get; set; } = jobCode;
    public BatchedFeedbackBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a batched feedback baseline in status open.</summary>
    public static BatchedFeedbackBaseline NewBatchedFeedbackBaseline(string code, string title)
    {
        return new BatchedFeedbackBaseline(
            code,
            title,
            "",
            0,
            "",
            0,
            "",
            "",
            "",
            BatchedFeedbackBaselineStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedFeedbackBaselineStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedFeedbackBaselineStatus next)
    {
        switch (Status)
        {
            case BatchedFeedbackBaselineStatus.Open:
                return next == BatchedFeedbackBaselineStatus.InReview;
            case BatchedFeedbackBaselineStatus.InReview:
                switch (next)
                {
                    case BatchedFeedbackBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedFeedbackBaselineStatus.Resolved:
                switch (next)
                {
                    case BatchedFeedbackBaselineStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedFeedbackBaselineStatus.Reopened:
                return next == BatchedFeedbackBaselineStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedFeedbackBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched feedback baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"batched feedback baseline"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)EndsAt + (double)GraceDays, 2);
    }

    public BatchedFeedbackBaseline Copy()
    {
        return new BatchedFeedbackBaseline(
            Code,
            Title,
            Segment,
            GraceDays,
            Department,
            EndsAt,
            Currency,
            EmployeeId,
            JobCode,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
