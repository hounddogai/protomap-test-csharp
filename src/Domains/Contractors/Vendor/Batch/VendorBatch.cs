namespace Acme.Workforce.Contractors.Vendor.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor batch is in its life cycle.</summary>
public enum VendorBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a vendor batch and its current status.</summary>
public sealed class VendorBatch(string code, string label, string phone, string summary, int version, bool active, VendorBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public int Version { get; set; } = version;
    public bool Active { get; set; } = active;
    public VendorBatchStatus Status { get; set; } = status;

    /// <summary>Starts a vendor batch in status new.</summary>
    public static VendorBatch NewVendorBatch(string code, string label)
    {
        return new VendorBatch(code, label, "", "", 0, false, VendorBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorBatchStatus next)
    {
        switch (Status)
        {
            case VendorBatchStatus.New:
                switch (next)
                {
                    case VendorBatchStatus.Running:
                    case VendorBatchStatus.Failed:
                        return true;
                }
                return false;
            case VendorBatchStatus.Queued:
                switch (next)
                {
                    case VendorBatchStatus.Running:
                        return true;
                }
                return false;
            case VendorBatchStatus.Running:
                switch (next)
                {
                    case VendorBatchStatus.Done:
                        return true;
                }
                return false;
            case VendorBatchStatus.Done:
                return next == VendorBatchStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VendorBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{"vendor batch"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public VendorBatch Copy()
    {
        return new VendorBatch(Code, Label, Phone, Summary, Version, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
