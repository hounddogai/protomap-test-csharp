namespace Acme.Workforce.Compliance.Audit.Checkpoint;

using Acme.Workforce.Kit.Text;

/// <summary>Where a audit item is in its life cycle.</summary>
public enum AuditItemStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A audit item. Its code is unique within a tenant.</summary>
public sealed class AuditItem(string code, string label, bool pinned, string title, bool visible, double minimum, double threshold, int capacity, string reviewer, string notes, double margin, AuditItemStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public double Minimum { get; set; } = minimum;
    public double Threshold { get; set; } = threshold;
    public int Capacity { get; set; } = capacity;
    public string Reviewer { get; set; } = reviewer;
    public string Notes { get; set; } = notes;
    public double Margin { get; set; } = margin;
    public AuditItemStatus Status { get; set; } = status;

    /// <summary>Starts a audit item in status proposed.</summary>
    public static AuditItem NewAuditItem(string code, string label)
    {
        return new AuditItem(code, label, false, "", false, 0.0, 0.0, 0, "", "", 0.0, AuditItemStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AuditItemStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AuditItemStatus next)
    {
        switch (Status)
        {
            case AuditItemStatus.Proposed:
                switch (next)
                {
                    case AuditItemStatus.Approved:
                    case AuditItemStatus.Rejected:
                        return true;
                }
                return false;
            case AuditItemStatus.Approved:
                return next == AuditItemStatus.Rejected || next == AuditItemStatus.Withdrawn;
            case AuditItemStatus.Rejected:
                return next == AuditItemStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(AuditItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"audit item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin - (double)Capacity, 2);
    }

    public AuditItem Copy()
    {
        return new AuditItem(
            Code,
            Label,
            Pinned,
            Title,
            Visible,
            Minimum,
            Threshold,
            Capacity,
            Reviewer,
            Notes,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
