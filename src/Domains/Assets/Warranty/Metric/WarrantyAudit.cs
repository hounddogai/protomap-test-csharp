namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty audit is in its life cycle.</summary>
public enum WarrantyAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A warranty audit. Its code is unique within a tenant.</summary>
public sealed class WarrantyAudit(string code, string name, double discount, int leaveDays, string summary, int version, double hours, string unit, string phone, double floorValue, int endsAt, WarrantyAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public int LeaveDays { get; set; } = leaveDays;
    public string Summary { get; set; } = summary;
    public int Version { get; set; } = version;
    public double Hours { get; set; } = hours;
    public string Unit { get; set; } = unit;
    public string Phone { get; set; } = phone;
    public double FloorValue { get; set; } = floorValue;
    public int EndsAt { get; set; } = endsAt;
    public WarrantyAuditStatus Status { get; set; } = status;

    /// <summary>Starts a warranty audit in status planned.</summary>
    public static WarrantyAudit NewWarrantyAudit(string code, string name)
    {
        return new WarrantyAudit(code, name, 0.0, 0, "", 0, 0.0, "", "", 0.0, 0, WarrantyAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyAuditStatus next)
    {
        switch (Status)
        {
            case WarrantyAuditStatus.Planned:
                return next == WarrantyAuditStatus.Cancelled;
            case WarrantyAuditStatus.Scheduled:
                return next == WarrantyAuditStatus.InProgress || next == WarrantyAuditStatus.Cancelled;
            case WarrantyAuditStatus.InProgress:
                return next == WarrantyAuditStatus.Completed;
            case WarrantyAuditStatus.Completed:
                switch (next)
                {
                    case WarrantyAuditStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round(Discount + (double)EndsAt, 2);
    }

    public WarrantyAudit Copy()
    {
        return new WarrantyAudit(
            Code,
            Name,
            Discount,
            LeaveDays,
            Summary,
            Version,
            Hours,
            Unit,
            Phone,
            FloorValue,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
