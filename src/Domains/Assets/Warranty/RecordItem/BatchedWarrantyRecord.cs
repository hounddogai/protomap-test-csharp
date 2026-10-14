namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched warranty record is in its life cycle.</summary>
public enum BatchedWarrantyRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One batched warranty record, with what the planners need to act on it.</summary>
public sealed class BatchedWarrantyRecord(string code, string title, double minimum, int tenureMonths, string reviewer, double ceiling, bool archived, BatchedWarrantyRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Reviewer { get; set; } = reviewer;
    public double Ceiling { get; set; } = ceiling;
    public bool Archived { get; set; } = archived;
    public BatchedWarrantyRecordStatus Status { get; set; } = status;

    /// <summary>Starts a batched warranty record in status open.</summary>
    public static BatchedWarrantyRecord NewBatchedWarrantyRecord(string code, string title)
    {
        return new BatchedWarrantyRecord(code, title, 0.0, 0, "", 0.0, false, BatchedWarrantyRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedWarrantyRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedWarrantyRecordStatus next)
    {
        switch (Status)
        {
            case BatchedWarrantyRecordStatus.Open:
                switch (next)
                {
                    case BatchedWarrantyRecordStatus.InReview:
                    case BatchedWarrantyRecordStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedWarrantyRecordStatus.InReview:
                switch (next)
                {
                    case BatchedWarrantyRecordStatus.Resolved:
                    case BatchedWarrantyRecordStatus.Archived:
                        return true;
                }
                return false;
            case BatchedWarrantyRecordStatus.Resolved:
                return next == BatchedWarrantyRecordStatus.Archived;
            case BatchedWarrantyRecordStatus.Reopened:
                return next == BatchedWarrantyRecordStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BatchedWarrantyRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched warranty record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetExposure()
    {
        return Math.Round(Ceiling * Minimum, 2);
    }

    public BatchedWarrantyRecord Copy()
    {
        return new BatchedWarrantyRecord(Code, Title, Minimum, TenureMonths, Reviewer, Ceiling, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
