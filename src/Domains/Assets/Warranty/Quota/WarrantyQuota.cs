namespace Acme.Workforce.Assets.Warranty.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty quota is in its life cycle.</summary>
public enum WarrantyQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A warranty quota as the warranty team keeps it.</summary>
public sealed class WarrantyQuota(string code, string name, string unit, List<string> tags, int createdAt, WarrantyQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public List<string> Tags { get; set; } = tags;
    public int CreatedAt { get; set; } = createdAt;
    public WarrantyQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a warranty quota in status proposed.</summary>
    public static WarrantyQuota NewWarrantyQuota(string code, string name)
    {
        return new WarrantyQuota(code, name, "", new List<string>(), 0, WarrantyQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyQuotaStatus next)
    {
        switch (Status)
        {
            case WarrantyQuotaStatus.Proposed:
                switch (next)
                {
                    case WarrantyQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case WarrantyQuotaStatus.Approved:
                return next == WarrantyQuotaStatus.Rejected;
            case WarrantyQuotaStatus.Rejected:
                switch (next)
                {
                    case WarrantyQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double MeasuredValue()
    {
        return (double)CreatedAt;
    }

    public WarrantyQuota Copy()
    {
        return new WarrantyQuota(Code, Name, Unit, Tags, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
