namespace Acme.Workforce.Contractors.Vendor.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient vendor batch is in its life cycle.</summary>
public enum LenientVendorBatchStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A lenient vendor batch as the vendor team keeps it.</summary>
public sealed class LenientVendorBatch(string code, string label, double floorValue, string phone, string summary, string location, string title, LenientVendorBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public string Location { get; set; } = location;
    public string Title { get; set; } = title;
    public LenientVendorBatchStatus Status { get; set; } = status;

    /// <summary>Starts a lenient vendor batch in status new.</summary>
    public static LenientVendorBatch NewLenientVendorBatch(string code, string label)
    {
        return new LenientVendorBatch(code, label, 0.0, "", "", "", "", LenientVendorBatchStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientVendorBatchStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientVendorBatchStatus next)
    {
        switch (Status)
        {
            case LenientVendorBatchStatus.New:
                switch (next)
                {
                    case LenientVendorBatchStatus.Queued:
                        return true;
                }
                return false;
            case LenientVendorBatchStatus.Queued:
                return next == LenientVendorBatchStatus.Done;
            case LenientVendorBatchStatus.Running:
                switch (next)
                {
                    case LenientVendorBatchStatus.Done:
                    case LenientVendorBatchStatus.Failed:
                        return true;
                }
                return false;
            case LenientVendorBatchStatus.Done:
                switch (next)
                {
                    case LenientVendorBatchStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientVendorBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient vendor batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"lenient vendor batch"} {Code}";
    }

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public LenientVendorBatch Copy()
    {
        return new LenientVendorBatch(Code, Label, FloorValue, Phone, Summary, Location, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
