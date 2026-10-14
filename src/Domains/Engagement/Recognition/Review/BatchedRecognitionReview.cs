namespace Acme.Workforce.Engagement.Recognition.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched recognition review is in its life cycle.</summary>
public enum BatchedRecognitionReviewStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched recognition review. Its code is unique within a tenant.</summary>
public sealed class BatchedRecognitionReview(string code, string name, bool visible, int graceDays, double ceiling, bool active, string externalRef, BatchedRecognitionReviewStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public int GraceDays { get; set; } = graceDays;
    public double Ceiling { get; set; } = ceiling;
    public bool Active { get; set; } = active;
    public string ExternalRef { get; set; } = externalRef;
    public BatchedRecognitionReviewStatus Status { get; set; } = status;

    /// <summary>Starts a batched recognition review in status new.</summary>
    public static BatchedRecognitionReview NewBatchedRecognitionReview(string code, string name)
    {
        return new BatchedRecognitionReview(
            code,
            name,
            false,
            0,
            0.0,
            false,
            "",
            BatchedRecognitionReviewStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedRecognitionReviewStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedRecognitionReviewStatus next)
    {
        switch (Status)
        {
            case BatchedRecognitionReviewStatus.New:
                switch (next)
                {
                    case BatchedRecognitionReviewStatus.Running:
                    case BatchedRecognitionReviewStatus.Done:
                        return true;
                }
                return false;
            case BatchedRecognitionReviewStatus.Queued:
                return next == BatchedRecognitionReviewStatus.Running;
            case BatchedRecognitionReviewStatus.Running:
                return next == BatchedRecognitionReviewStatus.Done || next == BatchedRecognitionReviewStatus.Failed;
            case BatchedRecognitionReviewStatus.Done:
                switch (next)
                {
                    case BatchedRecognitionReviewStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedRecognitionReviewStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched recognition review {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)GraceDays * Ceiling, 2);
    }

    public BatchedRecognitionReview Copy()
    {
        return new BatchedRecognitionReview(Code, Name, Visible, GraceDays, Ceiling, Active, ExternalRef, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
