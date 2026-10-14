namespace Acme.Workforce.Contractors.Vendor.VendorTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched vendor template is in its life cycle.</summary>
public enum BatchedVendorTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One batched vendor template, with what the planners need to act on it.</summary>
public sealed class BatchedVendorTemplate(string code, string title, string salaryBand, int revision, double margin, double ceiling, int attempts, int batchSize, List<string> tags, int quantity, BatchedVendorTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public int Revision { get; set; } = revision;
    public double Margin { get; set; } = margin;
    public double Ceiling { get; set; } = ceiling;
    public int Attempts { get; set; } = attempts;
    public int BatchSize { get; set; } = batchSize;
    public List<string> Tags { get; set; } = tags;
    public int Quantity { get; set; } = quantity;
    public BatchedVendorTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a batched vendor template in status planned.</summary>
    public static BatchedVendorTemplate NewBatchedVendorTemplate(string code, string title)
    {
        return new BatchedVendorTemplate(
            code,
            title,
            "",
            0,
            0.0,
            0.0,
            0,
            0,
            new List<string>(),
            0,
            BatchedVendorTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedVendorTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedVendorTemplateStatus next)
    {
        switch (Status)
        {
            case BatchedVendorTemplateStatus.Planned:
                return next == BatchedVendorTemplateStatus.Scheduled || next == BatchedVendorTemplateStatus.InProgress;
            case BatchedVendorTemplateStatus.Scheduled:
                return next == BatchedVendorTemplateStatus.InProgress;
            case BatchedVendorTemplateStatus.InProgress:
                return next == BatchedVendorTemplateStatus.Completed || next == BatchedVendorTemplateStatus.Cancelled;
            case BatchedVendorTemplateStatus.Completed:
                return next == BatchedVendorTemplateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BatchedVendorTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched vendor template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedValue()
    {
        return Math.Round(Margin * (double)BatchSize, 2);
    }

    public BatchedVendorTemplate Copy()
    {
        return new BatchedVendorTemplate(
            Code,
            Title,
            SalaryBand,
            Revision,
            Margin,
            Ceiling,
            Attempts,
            BatchSize,
            Tags,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
