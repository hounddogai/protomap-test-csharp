namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default sentiment assignment is in its life cycle.</summary>
public enum DefaultSentimentAssignmentStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One default sentiment assignment, with what the planners need to act on it.</summary>
public sealed class DefaultSentimentAssignment(string code, string title, double score, string category, string employeeId, DefaultSentimentAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string Category { get; set; } = category;
    public string EmployeeId { get; set; } = employeeId;
    public DefaultSentimentAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a default sentiment assignment in status requested.</summary>
    public static DefaultSentimentAssignment NewDefaultSentimentAssignment(string code, string title)
    {
        return new DefaultSentimentAssignment(code, title, 0.0, "", "", DefaultSentimentAssignmentStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSentimentAssignmentStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSentimentAssignmentStatus next)
    {
        switch (Status)
        {
            case DefaultSentimentAssignmentStatus.Requested:
                return next == DefaultSentimentAssignmentStatus.Revoked || next == DefaultSentimentAssignmentStatus.Expired;
            case DefaultSentimentAssignmentStatus.Granted:
                switch (next)
                {
                    case DefaultSentimentAssignmentStatus.Revoked:
                    case DefaultSentimentAssignmentStatus.Expired:
                        return true;
                }
                return false;
            case DefaultSentimentAssignmentStatus.Revoked:
                switch (next)
                {
                    case DefaultSentimentAssignmentStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSentimentAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default sentiment assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public DefaultSentimentAssignment Copy()
    {
        return new DefaultSentimentAssignment(Code, Title, Score, Category, EmployeeId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
