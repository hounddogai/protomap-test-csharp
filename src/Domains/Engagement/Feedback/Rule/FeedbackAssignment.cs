namespace Acme.Workforce.Engagement.Feedback.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a feedback assignment is in its life cycle.</summary>
public enum FeedbackAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A feedback assignment. Its code is unique within a tenant.</summary>
public sealed class FeedbackAssignment(string code, string name, string label, string category, int quantity, string reviewer, double maximum, string region, string ownerId, double score, int capacity, FeedbackAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public int Quantity { get; set; } = quantity;
    public string Reviewer { get; set; } = reviewer;
    public double Maximum { get; set; } = maximum;
    public string Region { get; set; } = region;
    public string OwnerId { get; set; } = ownerId;
    public double Score { get; set; } = score;
    public int Capacity { get; set; } = capacity;
    public FeedbackAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a feedback assignment in status draft.</summary>
    public static FeedbackAssignment NewFeedbackAssignment(string code, string name)
    {
        return new FeedbackAssignment(
            code,
            name,
            "",
            "",
            0,
            "",
            0.0,
            "",
            "",
            0.0,
            0,
            FeedbackAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FeedbackAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FeedbackAssignmentStatus next)
    {
        switch (Status)
        {
            case FeedbackAssignmentStatus.Draft:
                switch (next)
                {
                    case FeedbackAssignmentStatus.Active:
                    case FeedbackAssignmentStatus.Suspended:
                        return true;
                }
                return false;
            case FeedbackAssignmentStatus.Pending:
                switch (next)
                {
                    case FeedbackAssignmentStatus.Active:
                        return true;
                }
                return false;
            case FeedbackAssignmentStatus.Active:
                return next == FeedbackAssignmentStatus.Suspended || next == FeedbackAssignmentStatus.Closed;
            case FeedbackAssignmentStatus.Suspended:
                return next == FeedbackAssignmentStatus.Closed;
        }
        return false;
    }

    public void MoveTo(FeedbackAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"feedback assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Quantity - (double)Capacity, 2);
    }

    public FeedbackAssignment Copy()
    {
        return new FeedbackAssignment(
            Code,
            Name,
            Label,
            Category,
            Quantity,
            Reviewer,
            Maximum,
            Region,
            OwnerId,
            Score,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
