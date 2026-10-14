namespace Acme.Workforce.Assets.Assignment.AssignmentSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched assignment entry is in its life cycle.</summary>
public enum BatchedAssignmentEntryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a batched assignment entry and its current status.</summary>
public sealed class BatchedAssignmentEntry(string code, string title, double minimum, int batchSize, int capacity, double margin, bool approved, BatchedAssignmentEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public int BatchSize { get; set; } = batchSize;
    public int Capacity { get; set; } = capacity;
    public double Margin { get; set; } = margin;
    public bool Approved { get; set; } = approved;
    public BatchedAssignmentEntryStatus Status { get; set; } = status;

    /// <summary>Starts a batched assignment entry in status draft.</summary>
    public static BatchedAssignmentEntry NewBatchedAssignmentEntry(string code, string title)
    {
        return new BatchedAssignmentEntry(code, title, 0.0, 0, 0, 0.0, false, BatchedAssignmentEntryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedAssignmentEntryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedAssignmentEntryStatus next)
    {
        switch (Status)
        {
            case BatchedAssignmentEntryStatus.Draft:
                switch (next)
                {
                    case BatchedAssignmentEntryStatus.Active:
                    case BatchedAssignmentEntryStatus.Suspended:
                        return true;
                }
                return false;
            case BatchedAssignmentEntryStatus.Pending:
                switch (next)
                {
                    case BatchedAssignmentEntryStatus.Active:
                    case BatchedAssignmentEntryStatus.Suspended:
                        return true;
                }
                return false;
            case BatchedAssignmentEntryStatus.Active:
                return next == BatchedAssignmentEntryStatus.Closed;
            case BatchedAssignmentEntryStatus.Suspended:
                return next == BatchedAssignmentEntryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BatchedAssignmentEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched assignment entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Minimum + (double)Capacity, 2);
    }

    public BatchedAssignmentEntry Copy()
    {
        return new BatchedAssignmentEntry(Code, Title, Minimum, BatchSize, Capacity, Margin, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
