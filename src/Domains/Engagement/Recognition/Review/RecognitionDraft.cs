namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a recognition draft is in its life cycle.</summary>
public enum RecognitionDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A recognition draft as the recognition team keeps it.</summary>
public sealed class RecognitionDraft(string code, string name, string segment, int updatedAt, int expiresAt, bool pinned, string unit, bool visible, int batchSize, double rating, string timezone, RecognitionDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public int UpdatedAt { get; set; } = updatedAt;
    public int ExpiresAt { get; set; } = expiresAt;
    public bool Pinned { get; set; } = pinned;
    public string Unit { get; set; } = unit;
    public bool Visible { get; set; } = visible;
    public int BatchSize { get; set; } = batchSize;
    public double Rating { get; set; } = rating;
    public string Timezone { get; set; } = timezone;
    public RecognitionDraftStatus Status { get; set; } = status;

    /// <summary>Starts a recognition draft in status new.</summary>
    public static RecognitionDraft NewRecognitionDraft(string code, string name)
    {
        return new RecognitionDraft(code, name, "", 0, 0, false, "", false, 0, 0.0, "", RecognitionDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RecognitionDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RecognitionDraftStatus next)
    {
        switch (Status)
        {
            case RecognitionDraftStatus.New:
                switch (next)
                {
                    case RecognitionDraftStatus.Failed:
                        return true;
                }
                return false;
            case RecognitionDraftStatus.Queued:
                switch (next)
                {
                    case RecognitionDraftStatus.Done:
                    case RecognitionDraftStatus.Failed:
                        return true;
                }
                return false;
            case RecognitionDraftStatus.Running:
                switch (next)
                {
                    case RecognitionDraftStatus.Done:
                        return true;
                }
                return false;
            case RecognitionDraftStatus.Done:
                switch (next)
                {
                    case RecognitionDraftStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RecognitionDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"recognition draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)BatchSize + (double)ExpiresAt, 2);
    }

    public RecognitionDraft Copy()
    {
        return new RecognitionDraft(
            Code,
            Name,
            Segment,
            UpdatedAt,
            ExpiresAt,
            Pinned,
            Unit,
            Visible,
            BatchSize,
            Rating,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
