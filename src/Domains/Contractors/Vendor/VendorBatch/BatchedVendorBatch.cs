namespace Acme.Workforce.Contractors.Vendor.VendorBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched vendor batch is in its life cycle.</summary>
public enum BatchedVendorBatchStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A batched vendor batch as the vendor team keeps it.</summary>
public sealed class BatchedVendorBatch(string code, string label, double weight, int leaveDays, double score, string badge, string grade, string checksum, BatchedVendorBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Weight { get; set; } = weight;
    public int LeaveDays { get; set; } = leaveDays;
    public double Score { get; set; } = score;
    public string Badge { get; set; } = badge;
    public string Grade { get; set; } = grade;
    public string Checksum { get; set; } = checksum;
    public BatchedVendorBatchStatus Status { get; set; } = status;

    /// <summary>Starts a batched vendor batch in status proposed.</summary>
    public static BatchedVendorBatch NewBatchedVendorBatch(string code, string label)
    {
        return new BatchedVendorBatch(code, label, 0.0, 0, 0.0, "", "", "", BatchedVendorBatchStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedVendorBatchStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedVendorBatchStatus next)
    {
        switch (Status)
        {
            case BatchedVendorBatchStatus.Proposed:
                return next == BatchedVendorBatchStatus.Approved || next == BatchedVendorBatchStatus.Withdrawn;
            case BatchedVendorBatchStatus.Approved:
                switch (next)
                {
                    case BatchedVendorBatchStatus.Rejected:
                    case BatchedVendorBatchStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedVendorBatchStatus.Rejected:
                return next == BatchedVendorBatchStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedVendorBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched vendor batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)LeaveDays * Score, 2);
    }

    public BatchedVendorBatch Copy()
    {
        return new BatchedVendorBatch(Code, Label, Weight, LeaveDays, Score, Badge, Grade, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
