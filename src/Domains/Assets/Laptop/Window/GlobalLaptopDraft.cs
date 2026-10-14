namespace Acme.Workforce.Assets.Laptop.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global laptop draft is in its life cycle.</summary>
public enum GlobalLaptopDraftStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global laptop draft. Its code is unique within a tenant.</summary>
public sealed class GlobalLaptopDraft(string code, string label, string currency, string externalRef, int batchSize, string phone, double maximum, string checksum, string employeeId, GlobalLaptopDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public string ExternalRef { get; set; } = externalRef;
    public int BatchSize { get; set; } = batchSize;
    public string Phone { get; set; } = phone;
    public double Maximum { get; set; } = maximum;
    public string Checksum { get; set; } = checksum;
    public string EmployeeId { get; set; } = employeeId;
    public GlobalLaptopDraftStatus Status { get; set; } = status;

    /// <summary>Starts a global laptop draft in status planned.</summary>
    public static GlobalLaptopDraft NewGlobalLaptopDraft(string code, string label)
    {
        return new GlobalLaptopDraft(code, label, "", "", 0, "", 0.0, "", "", GlobalLaptopDraftStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalLaptopDraftStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalLaptopDraftStatus next)
    {
        switch (Status)
        {
            case GlobalLaptopDraftStatus.Planned:
                switch (next)
                {
                    case GlobalLaptopDraftStatus.Completed:
                    case GlobalLaptopDraftStatus.Cancelled:
                        return true;
                }
                return false;
            case GlobalLaptopDraftStatus.Scheduled:
                switch (next)
                {
                    case GlobalLaptopDraftStatus.Completed:
                        return true;
                }
                return false;
            case GlobalLaptopDraftStatus.InProgress:
                return next == GlobalLaptopDraftStatus.Completed || next == GlobalLaptopDraftStatus.Cancelled;
            case GlobalLaptopDraftStatus.Completed:
                switch (next)
                {
                    case GlobalLaptopDraftStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalLaptopDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global laptop draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)BatchSize * Maximum, 2);
    }

    public GlobalLaptopDraft Copy()
    {
        return new GlobalLaptopDraft(
            Code,
            Label,
            Currency,
            ExternalRef,
            BatchSize,
            Phone,
            Maximum,
            Checksum,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
