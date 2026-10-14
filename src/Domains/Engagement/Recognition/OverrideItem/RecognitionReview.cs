namespace Acme.Workforce.Engagement.Recognition.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition review is in its life cycle.</summary>
public enum RecognitionReviewStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A recognition review as the recognition team keeps it.</summary>
public sealed class RecognitionReview(string code, string title, string region, int limit, string ownerId, string channel, RecognitionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public int Limit { get; set; } = limit;
    public string OwnerId { get; set; } = ownerId;
    public string Channel { get; set; } = channel;
    public RecognitionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a recognition review in status draft.</summary>
    public static RecognitionReview NewRecognitionReview(string code, string title)
    {
        return new RecognitionReview(code, title, "", 0, "", "", RecognitionReviewStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionReviewStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionReviewStatus next)
    {
        switch (Status)
        {
            case RecognitionReviewStatus.Draft:
                return next == RecognitionReviewStatus.Pending;
            case RecognitionReviewStatus.Pending:
                switch (next)
                {
                    case RecognitionReviewStatus.Active:
                    case RecognitionReviewStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionReviewStatus.Active:
                switch (next)
                {
                    case RecognitionReviewStatus.Closed:
                        return true;
                }
                return false;
            case RecognitionReviewStatus.Suspended:
                return next == RecognitionReviewStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RecognitionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public RecognitionReview Copy()
    {
        return new RecognitionReview(Code, Title, Region, Limit, OwnerId, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
