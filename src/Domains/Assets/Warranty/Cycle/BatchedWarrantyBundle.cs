namespace Acme.Workforce.Assets.Warranty.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched warranty bundle is in its life cycle.</summary>
public enum BatchedWarrantyBundleStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A batched warranty bundle. Its code is unique within a tenant.</summary>
public sealed class BatchedWarrantyBundle(string code, string name, string contactEmail, string checksum, string department, double amount, BatchedWarrantyBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ContactEmail { get; set; } = contactEmail;
    public string Checksum { get; set; } = checksum;
    public string Department { get; set; } = department;
    public double Amount { get; set; } = amount;
    public BatchedWarrantyBundleStatus Status { get; set; } = status;

    /// <summary>Starts a batched warranty bundle in status new.</summary>
    public static BatchedWarrantyBundle NewBatchedWarrantyBundle(string code, string name)
    {
        return new BatchedWarrantyBundle(code, name, "", "", "", 0.0, BatchedWarrantyBundleStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedWarrantyBundleStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedWarrantyBundleStatus next)
    {
        switch (Status)
        {
            case BatchedWarrantyBundleStatus.New:
                switch (next)
                {
                    case BatchedWarrantyBundleStatus.Queued:
                        return true;
                }
                return false;
            case BatchedWarrantyBundleStatus.Queued:
                switch (next)
                {
                    case BatchedWarrantyBundleStatus.Running:
                    case BatchedWarrantyBundleStatus.Failed:
                        return true;
                }
                return false;
            case BatchedWarrantyBundleStatus.Running:
                return next == BatchedWarrantyBundleStatus.Done;
            case BatchedWarrantyBundleStatus.Done:
                return next == BatchedWarrantyBundleStatus.Failed;
        }
        return false;
    }

    public void MoveTo(BatchedWarrantyBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched warranty bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public BatchedWarrantyBundle Copy()
    {
        return new BatchedWarrantyBundle(Code, Name, ContactEmail, Checksum, Department, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
