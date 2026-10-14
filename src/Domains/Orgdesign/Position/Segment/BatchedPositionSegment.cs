namespace Acme.Workforce.Orgdesign.Position.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched position segment is in its life cycle.</summary>
public enum BatchedPositionSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a batched position segment and its current status.</summary>
public sealed class BatchedPositionSegment(string code, string title, string category, int attempts, string reviewer, string costCenter, string phone, double budget, int capacity, string grade, string name, BatchedPositionSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Category { get; set; } = category;
    public int Attempts { get; set; } = attempts;
    public string Reviewer { get; set; } = reviewer;
    public string CostCenter { get; set; } = costCenter;
    public string Phone { get; set; } = phone;
    public double Budget { get; set; } = budget;
    public int Capacity { get; set; } = capacity;
    public string Grade { get; set; } = grade;
    public string Name { get; set; } = name;
    public BatchedPositionSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a batched position segment in status new.</summary>
    public static BatchedPositionSegment NewBatchedPositionSegment(string code, string title)
    {
        return new BatchedPositionSegment(
            code,
            title,
            "",
            0,
            "",
            "",
            "",
            0.0,
            0,
            "",
            "",
            BatchedPositionSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedPositionSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedPositionSegmentStatus next)
    {
        switch (Status)
        {
            case BatchedPositionSegmentStatus.New:
                switch (next)
                {
                    case BatchedPositionSegmentStatus.Queued:
                    case BatchedPositionSegmentStatus.Done:
                        return true;
                }
                return false;
            case BatchedPositionSegmentStatus.Queued:
                switch (next)
                {
                    case BatchedPositionSegmentStatus.Running:
                        return true;
                }
                return false;
            case BatchedPositionSegmentStatus.Running:
                return next == BatchedPositionSegmentStatus.Done || next == BatchedPositionSegmentStatus.Failed;
            case BatchedPositionSegmentStatus.Done:
                return next == BatchedPositionSegmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedPositionSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched position segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Capacity - (double)Attempts, 2);
    }

    public BatchedPositionSegment Copy()
    {
        return new BatchedPositionSegment(
            Code,
            Title,
            Category,
            Attempts,
            Reviewer,
            CostCenter,
            Phone,
            Budget,
            Capacity,
            Grade,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
