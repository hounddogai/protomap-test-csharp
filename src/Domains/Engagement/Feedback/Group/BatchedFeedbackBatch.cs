namespace Acme.Workforce.Engagement.Feedback.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched feedback batch is in its life cycle.</summary>
public enum BatchedFeedbackBatchStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One batched feedback batch, with what the planners need to act on it.</summary>
public sealed class BatchedFeedbackBatch(string code, string title, string category, string segment, double margin, string notes, string employeeId, double discount, double amount, string reviewer, BatchedFeedbackBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public string Notes { get; set; } = notes;
    public string EmployeeId { get; set; } = employeeId;
    public double Discount { get; set; } = discount;
    public double Amount { get; set; } = amount;
    public string Reviewer { get; set; } = reviewer;
    public BatchedFeedbackBatchStatus Status { get; set; } = status;

    /// <summary>Starts a batched feedback batch in status draft.</summary>
    public static BatchedFeedbackBatch NewBatchedFeedbackBatch(string code, string title)
    {
        return new BatchedFeedbackBatch(
            code,
            title,
            "",
            "",
            0.0,
            "",
            "",
            0.0,
            0.0,
            "",
            BatchedFeedbackBatchStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedFeedbackBatchStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedFeedbackBatchStatus next)
    {
        switch (Status)
        {
            case BatchedFeedbackBatchStatus.Draft:
                return next == BatchedFeedbackBatchStatus.Active || next == BatchedFeedbackBatchStatus.Closed;
            case BatchedFeedbackBatchStatus.Pending:
                return next == BatchedFeedbackBatchStatus.Active || next == BatchedFeedbackBatchStatus.Suspended;
            case BatchedFeedbackBatchStatus.Active:
                switch (next)
                {
                    case BatchedFeedbackBatchStatus.Suspended:
                    case BatchedFeedbackBatchStatus.Closed:
                        return true;
                }
                return false;
            case BatchedFeedbackBatchStatus.Suspended:
                return next == BatchedFeedbackBatchStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BatchedFeedbackBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched feedback batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetTotal()
    {
        return Math.Round(Discount * Amount, 2);
    }

    public BatchedFeedbackBatch Copy()
    {
        return new BatchedFeedbackBatch(
            Code,
            Title,
            Category,
            Segment,
            Margin,
            Notes,
            EmployeeId,
            Discount,
            Amount,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
