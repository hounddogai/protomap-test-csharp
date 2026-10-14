namespace Acme.Workforce.Compliance.Exception.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched exception record is in its life cycle.</summary>
public enum BatchedExceptionRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A batched exception record as the exception team keeps it.</summary>
public sealed class BatchedExceptionRecord(string code, string name, double threshold, string category, double margin, string costCenter, BatchedExceptionRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Threshold { get; set; } = threshold;
    public string Category { get; set; } = category;
    public double Margin { get; set; } = margin;
    public string CostCenter { get; set; } = costCenter;
    public BatchedExceptionRecordStatus Status { get; set; } = status;

    /// <summary>Starts a batched exception record in status open.</summary>
    public static BatchedExceptionRecord NewBatchedExceptionRecord(string code, string name)
    {
        return new BatchedExceptionRecord(code, name, 0.0, "", 0.0, "", BatchedExceptionRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedExceptionRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedExceptionRecordStatus next)
    {
        switch (Status)
        {
            case BatchedExceptionRecordStatus.Open:
                switch (next)
                {
                    case BatchedExceptionRecordStatus.Resolved:
                    case BatchedExceptionRecordStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedExceptionRecordStatus.InReview:
                switch (next)
                {
                    case BatchedExceptionRecordStatus.Resolved:
                    case BatchedExceptionRecordStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedExceptionRecordStatus.Resolved:
                switch (next)
                {
                    case BatchedExceptionRecordStatus.Reopened:
                    case BatchedExceptionRecordStatus.Archived:
                        return true;
                }
                return false;
            case BatchedExceptionRecordStatus.Reopened:
                switch (next)
                {
                    case BatchedExceptionRecordStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedExceptionRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched exception record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Margin * Threshold, 2);
    }

    public BatchedExceptionRecord Copy()
    {
        return new BatchedExceptionRecord(Code, Name, Threshold, Category, Margin, CostCenter, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
