namespace Acme.Workforce.Assets.Warranty.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty target is in its life cycle.</summary>
public enum WarrantyTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One warranty target, with what the planners need to act on it.</summary>
public sealed class WarrantyTarget(string code, string label, int revision, string category, bool active, int quantity, int tenureMonths, string externalRef, int leaveDays, double rating, WarrantyTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public string Category { get; set; } = category;
    public bool Active { get; set; } = active;
    public int Quantity { get; set; } = quantity;
    public int TenureMonths { get; set; } = tenureMonths;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public double Rating { get; set; } = rating;
    public WarrantyTargetStatus Status { get; set; } = status;

    /// <summary>Starts a warranty target in status planned.</summary>
    public static WarrantyTarget NewWarrantyTarget(string code, string label)
    {
        return new WarrantyTarget(code, label, 0, "", false, 0, 0, "", 0, 0.0, WarrantyTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyTargetStatus next)
    {
        switch (Status)
        {
            case WarrantyTargetStatus.Planned:
                return next == WarrantyTargetStatus.Completed;
            case WarrantyTargetStatus.Scheduled:
                switch (next)
                {
                    case WarrantyTargetStatus.Completed:
                        return true;
                }
                return false;
            case WarrantyTargetStatus.InProgress:
                switch (next)
                {
                    case WarrantyTargetStatus.Completed:
                    case WarrantyTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case WarrantyTargetStatus.Completed:
                return next == WarrantyTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(WarrantyTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Revision - (double)LeaveDays, 2);
    }

    public WarrantyTarget Copy()
    {
        return new WarrantyTarget(
            Code,
            Label,
            Revision,
            Category,
            Active,
            Quantity,
            TenureMonths,
            ExternalRef,
            LeaveDays,
            Rating,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
