namespace Acme.Workforce.Engagement.Sentiment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient sentiment exception is in its life cycle.</summary>
public enum LenientSentimentExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient sentiment exception as the sentiment team keeps it.</summary>
public sealed class LenientSentimentException(string code, string name, string jobCode, string contactEmail, int createdAt, string employeeId, double margin, List<string> tags, string summary, LenientSentimentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public int CreatedAt { get; set; } = createdAt;
    public string EmployeeId { get; set; } = employeeId;
    public double Margin { get; set; } = margin;
    public List<string> Tags { get; set; } = tags;
    public string Summary { get; set; } = summary;
    public LenientSentimentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a lenient sentiment exception in status new.</summary>
    public static LenientSentimentException NewLenientSentimentException(string code, string name)
    {
        return new LenientSentimentException(
            code,
            name,
            "",
            "",
            0,
            "",
            0.0,
            new List<string>(),
            "",
            LenientSentimentExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientSentimentExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientSentimentExceptionStatus next)
    {
        switch (Status)
        {
            case LenientSentimentExceptionStatus.New:
                return next == LenientSentimentExceptionStatus.Running || next == LenientSentimentExceptionStatus.Done;
            case LenientSentimentExceptionStatus.Queued:
                switch (next)
                {
                    case LenientSentimentExceptionStatus.Done:
                        return true;
                }
                return false;
            case LenientSentimentExceptionStatus.Running:
                return next == LenientSentimentExceptionStatus.Done || next == LenientSentimentExceptionStatus.Failed;
            case LenientSentimentExceptionStatus.Done:
                switch (next)
                {
                    case LenientSentimentExceptionStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientSentimentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient sentiment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetValue()
    {
        return Math.Round((double)CreatedAt * Margin, 2);
    }

    public LenientSentimentException Copy()
    {
        return new LenientSentimentException(
            Code,
            Name,
            JobCode,
            ContactEmail,
            CreatedAt,
            EmployeeId,
            Margin,
            Tags,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
