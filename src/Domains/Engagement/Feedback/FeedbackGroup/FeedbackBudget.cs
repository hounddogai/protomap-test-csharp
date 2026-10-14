namespace Acme.Workforce.Engagement.Feedback.FeedbackGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback budget is in its life cycle.</summary>
public enum FeedbackBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A feedback budget. Its code is unique within a tenant.</summary>
public sealed class FeedbackBudget(string code, string label, string jobCode, string segment, int createdAt, FeedbackBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public string Segment { get; set; } = segment;
    public int CreatedAt { get; set; } = createdAt;
    public FeedbackBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a feedback budget in status draft.</summary>
    public static FeedbackBudget NewFeedbackBudget(string code, string label)
    {
        return new FeedbackBudget(code, label, "", "", 0, FeedbackBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackBudgetStatus next)
    {
        switch (Status)
        {
            case FeedbackBudgetStatus.Draft:
                switch (next)
                {
                    case FeedbackBudgetStatus.Suspended:
                        return true;
                }
                return false;
            case FeedbackBudgetStatus.Pending:
                return next == FeedbackBudgetStatus.Active || next == FeedbackBudgetStatus.Closed;
            case FeedbackBudgetStatus.Active:
                return next == FeedbackBudgetStatus.Suspended;
            case FeedbackBudgetStatus.Suspended:
                switch (next)
                {
                    case FeedbackBudgetStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FeedbackBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"feedback budget"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public FeedbackBudget Copy()
    {
        return new FeedbackBudget(Code, Label, JobCode, Segment, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
