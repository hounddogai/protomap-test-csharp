namespace Acme.Workforce.Assets.Warranty.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped warranty baseline is in its life cycle.</summary>
public enum CappedWarrantyBaselineStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A capped warranty baseline as the warranty team keeps it.</summary>
public sealed class CappedWarrantyBaseline(string code, string label, string category, string notes, string region, int revision, int startsAt, int limit, int quantity, CappedWarrantyBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Category { get; set; } = category;
    public string Notes { get; set; } = notes;
    public string Region { get; set; } = region;
    public int Revision { get; set; } = revision;
    public int StartsAt { get; set; } = startsAt;
    public int Limit { get; set; } = limit;
    public int Quantity { get; set; } = quantity;
    public CappedWarrantyBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a capped warranty baseline in status created.</summary>
    public static CappedWarrantyBaseline NewCappedWarrantyBaseline(string code, string label)
    {
        return new CappedWarrantyBaseline(code, label, "", "", "", 0, 0, 0, 0, CappedWarrantyBaselineStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedWarrantyBaselineStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedWarrantyBaselineStatus next)
    {
        switch (Status)
        {
            case CappedWarrantyBaselineStatus.Created:
                return next == CappedWarrantyBaselineStatus.Published;
            case CappedWarrantyBaselineStatus.Validated:
                return next == CappedWarrantyBaselineStatus.Published || next == CappedWarrantyBaselineStatus.Retired;
            case CappedWarrantyBaselineStatus.Published:
                switch (next)
                {
                    case CappedWarrantyBaselineStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedWarrantyBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped warranty baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"capped warranty baseline"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round((double)Limit + (double)StartsAt, 2);
    }

    public CappedWarrantyBaseline Copy()
    {
        return new CappedWarrantyBaseline(
            Code,
            Label,
            Category,
            Notes,
            Region,
            Revision,
            StartsAt,
            Limit,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
