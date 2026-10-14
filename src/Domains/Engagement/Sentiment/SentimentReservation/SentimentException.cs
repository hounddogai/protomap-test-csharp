namespace Acme.Workforce.Engagement.Sentiment.SentimentReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment exception is in its life cycle.</summary>
public enum SentimentExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A sentiment exception as the sentiment team keeps it.</summary>
public sealed class SentimentException(string code, string label, string grade, bool approved, double maximum, string title, SentimentExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Grade { get; set; } = grade;
    public bool Approved { get; set; } = approved;
    public double Maximum { get; set; } = maximum;
    public string Title { get; set; } = title;
    public SentimentExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment exception in status proposed.</summary>
    public static SentimentException NewSentimentException(string code, string label)
    {
        return new SentimentException(code, label, "", false, 0.0, "", SentimentExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentExceptionStatus next)
    {
        switch (Status)
        {
            case SentimentExceptionStatus.Proposed:
                return next == SentimentExceptionStatus.Approved || next == SentimentExceptionStatus.Withdrawn;
            case SentimentExceptionStatus.Approved:
                return next == SentimentExceptionStatus.Rejected;
            case SentimentExceptionStatus.Rejected:
                switch (next)
                {
                    case SentimentExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SentimentExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"sentiment exception"} {Code}";
    }

    public double MeasuredValue()
    {
        return Maximum;
    }

    public SentimentException Copy()
    {
        return new SentimentException(Code, Label, Grade, Approved, Maximum, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
