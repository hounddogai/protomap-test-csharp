namespace Acme.Workforce.Engagement.Recognition.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition digest is in its life cycle.</summary>
public enum RecognitionDigestStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One recognition digest, with what the planners need to act on it.</summary>
public sealed class RecognitionDigest(string code, string title, List<string> tags, string reviewer, double ratio, string segment, string region, double discount, RecognitionDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public List<string> Tags { get; set; } = tags;
    public string Reviewer { get; set; } = reviewer;
    public double Ratio { get; set; } = ratio;
    public string Segment { get; set; } = segment;
    public string Region { get; set; } = region;
    public double Discount { get; set; } = discount;
    public RecognitionDigestStatus Status { get; set; } = status;

    /// <summary>Starts a recognition digest in status open.</summary>
    public static RecognitionDigest NewRecognitionDigest(string code, string title)
    {
        return new RecognitionDigest(
            code,
            title,
            new List<string>(),
            "",
            0.0,
            "",
            "",
            0.0,
            RecognitionDigestStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionDigestStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionDigestStatus next)
    {
        switch (Status)
        {
            case RecognitionDigestStatus.Open:
                return next == RecognitionDigestStatus.Reopened;
            case RecognitionDigestStatus.InReview:
                return next == RecognitionDigestStatus.Reopened || next == RecognitionDigestStatus.Archived;
            case RecognitionDigestStatus.Resolved:
                switch (next)
                {
                    case RecognitionDigestStatus.Archived:
                        return true;
                }
                return false;
            case RecognitionDigestStatus.Reopened:
                switch (next)
                {
                    case RecognitionDigestStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"recognition digest"} {Code}";
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
        return Math.Round(Discount - Ratio, 2);
    }

    public RecognitionDigest Copy()
    {
        return new RecognitionDigest(Code, Title, Tags, Reviewer, Ratio, Segment, Region, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
