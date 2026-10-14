namespace Acme.Workforce.Payrollops.TaxForm.FormBundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending tax form threshold is in its life cycle.</summary>
public enum PendingTaxFormThresholdStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A pending tax form threshold as the tax form team keeps it.</summary>
public sealed class PendingTaxFormThreshold(string code, string label, string externalRef, string notes, string region, int retries, string summary, string checksum, PendingTaxFormThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public string Notes { get; set; } = notes;
    public string Region { get; set; } = region;
    public int Retries { get; set; } = retries;
    public string Summary { get; set; } = summary;
    public string Checksum { get; set; } = checksum;
    public PendingTaxFormThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a pending tax form threshold in status new.</summary>
    public static PendingTaxFormThreshold NewPendingTaxFormThreshold(string code, string label)
    {
        return new PendingTaxFormThreshold(code, label, "", "", "", 0, "", "", PendingTaxFormThresholdStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTaxFormThresholdStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTaxFormThresholdStatus next)
    {
        switch (Status)
        {
            case PendingTaxFormThresholdStatus.New:
                switch (next)
                {
                    case PendingTaxFormThresholdStatus.Done:
                        return true;
                }
                return false;
            case PendingTaxFormThresholdStatus.Queued:
                return next == PendingTaxFormThresholdStatus.Failed;
            case PendingTaxFormThresholdStatus.Running:
                return next == PendingTaxFormThresholdStatus.Done || next == PendingTaxFormThresholdStatus.Failed;
            case PendingTaxFormThresholdStatus.Done:
                return next == PendingTaxFormThresholdStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PendingTaxFormThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending tax form threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"pending tax form threshold"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Retries;
    }

    public PendingTaxFormThreshold Copy()
    {
        return new PendingTaxFormThreshold(
            Code,
            Label,
            ExternalRef,
            Notes,
            Region,
            Retries,
            Summary,
            Checksum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
