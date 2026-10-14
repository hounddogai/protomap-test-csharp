namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict warranty tier is in its life cycle.</summary>
public enum StrictWarrantyTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A strict warranty tier. Its code is unique within a tenant.</summary>
public sealed class StrictWarrantyTier(string code, string name, string summary, double margin, double rate, string department, int createdAt, double amount, string ownerId, string location, StrictWarrantyTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Summary { get; set; } = summary;
    public double Margin { get; set; } = margin;
    public double Rate { get; set; } = rate;
    public string Department { get; set; } = department;
    public int CreatedAt { get; set; } = createdAt;
    public double Amount { get; set; } = amount;
    public string OwnerId { get; set; } = ownerId;
    public string Location { get; set; } = location;
    public StrictWarrantyTierStatus Status { get; set; } = status;

    /// <summary>Starts a strict warranty tier in status proposed.</summary>
    public static StrictWarrantyTier NewStrictWarrantyTier(string code, string name)
    {
        return new StrictWarrantyTier(
            code,
            name,
            "",
            0.0,
            0.0,
            "",
            0,
            0.0,
            "",
            "",
            StrictWarrantyTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictWarrantyTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictWarrantyTierStatus next)
    {
        switch (Status)
        {
            case StrictWarrantyTierStatus.Proposed:
                return next == StrictWarrantyTierStatus.Withdrawn;
            case StrictWarrantyTierStatus.Approved:
                return next == StrictWarrantyTierStatus.Rejected;
            case StrictWarrantyTierStatus.Rejected:
                return next == StrictWarrantyTierStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(StrictWarrantyTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict warranty tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rate + (double)CreatedAt, 2);
    }

    public StrictWarrantyTier Copy()
    {
        return new StrictWarrantyTier(
            Code,
            Name,
            Summary,
            Margin,
            Rate,
            Department,
            CreatedAt,
            Amount,
            OwnerId,
            Location,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
