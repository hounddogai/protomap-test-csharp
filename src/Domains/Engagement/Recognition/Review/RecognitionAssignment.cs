namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition assignment is in its life cycle.</summary>
public enum RecognitionAssignmentStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One recognition assignment, with what the planners need to act on it.</summary>
public sealed class RecognitionAssignment(string code, string title, double minimum, int quantity, string jobCode, double cost, double score, int createdAt, RecognitionAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public string JobCode { get; set; } = jobCode;
    public double Cost { get; set; } = cost;
    public double Score { get; set; } = score;
    public int CreatedAt { get; set; } = createdAt;
    public RecognitionAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a recognition assignment in status draft.</summary>
    public static RecognitionAssignment NewRecognitionAssignment(string code, string title)
    {
        return new RecognitionAssignment(code, title, 0.0, 0, "", 0.0, 0.0, 0, RecognitionAssignmentStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionAssignmentStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionAssignmentStatus next)
    {
        switch (Status)
        {
            case RecognitionAssignmentStatus.Draft:
                switch (next)
                {
                    case RecognitionAssignmentStatus.Active:
                    case RecognitionAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionAssignmentStatus.Pending:
                switch (next)
                {
                    case RecognitionAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionAssignmentStatus.Active:
                switch (next)
                {
                    case RecognitionAssignmentStatus.Suspended:
                    case RecognitionAssignmentStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionAssignmentStatus.Suspended:
                switch (next)
                {
                    case RecognitionAssignmentStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"recognition assignment"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)CreatedAt - (double)Quantity, 2);
    }

    public RecognitionAssignment Copy()
    {
        return new RecognitionAssignment(Code, Title, Minimum, Quantity, JobCode, Cost, Score, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
