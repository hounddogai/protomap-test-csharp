namespace Acme.Workforce.Contractors.Vendor.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a vendor exception is in its life cycle.</summary>
public enum VendorExceptionStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A vendor exception as the vendor team keeps it.</summary>
public sealed class VendorException(string code, string label, string department, int startsAt, int retries, double threshold, VendorExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Department { get; set; } = department;
    public int StartsAt { get; set; } = startsAt;
    public int Retries { get; set; } = retries;
    public double Threshold { get; set; } = threshold;
    public VendorExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a vendor exception in status new.</summary>
    public static VendorException NewVendorException(string code, string label)
    {
        return new VendorException(code, label, "", 0, 0, 0.0, VendorExceptionStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VendorExceptionStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VendorExceptionStatus next)
    {
        switch (Status)
        {
            case VendorExceptionStatus.New:
                switch (next)
                {
                    case VendorExceptionStatus.Queued:
                    case VendorExceptionStatus.Done:
                        return true;
                }
                return false;
            case VendorExceptionStatus.Queued:
                return next == VendorExceptionStatus.Running;
            case VendorExceptionStatus.Running:
                switch (next)
                {
                    case VendorExceptionStatus.Failed:
                        return true;
                }
                return false;
            case VendorExceptionStatus.Done:
                return next == VendorExceptionStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VendorExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"vendor exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"vendor exception"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Threshold + (double)StartsAt, 2);
    }

    public VendorException Copy()
    {
        return new VendorException(Code, Label, Department, StartsAt, Retries, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
