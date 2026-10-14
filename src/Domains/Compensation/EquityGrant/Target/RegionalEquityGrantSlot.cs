namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional equity grant slot is in its life cycle.</summary>
public enum RegionalEquityGrantSlotStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A regional equity grant slot. Its code is unique within a tenant.</summary>
public sealed class RegionalEquityGrantSlot(string code, string label, string description, int quantity, int startsAt, RegionalEquityGrantSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public int StartsAt { get; set; } = startsAt;
    public RegionalEquityGrantSlotStatus Status { get; set; } = status;

    /// <summary>Starts a regional equity grant slot in status open.</summary>
    public static RegionalEquityGrantSlot NewRegionalEquityGrantSlot(string code, string label)
    {
        return new RegionalEquityGrantSlot(code, label, "", 0, 0, RegionalEquityGrantSlotStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalEquityGrantSlotStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalEquityGrantSlotStatus next)
    {
        switch (Status)
        {
            case RegionalEquityGrantSlotStatus.Open:
                switch (next)
                {
                    case RegionalEquityGrantSlotStatus.InReview:
                        return true;
                }
                return false;
            case RegionalEquityGrantSlotStatus.InReview:
                switch (next)
                {
                    case RegionalEquityGrantSlotStatus.Resolved:
                    case RegionalEquityGrantSlotStatus.Reopened:
                        return true;
                }
                return false;
            case RegionalEquityGrantSlotStatus.Resolved:
                switch (next)
                {
                    case RegionalEquityGrantSlotStatus.Reopened:
                    case RegionalEquityGrantSlotStatus.Archived:
                        return true;
                }
                return false;
            case RegionalEquityGrantSlotStatus.Reopened:
                return next == RegionalEquityGrantSlotStatus.Archived;
        }
        return false;
    }

    public void MoveTo(RegionalEquityGrantSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional equity grant slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Quantity - (double)StartsAt, 2);
    }

    public RegionalEquityGrantSlot Copy()
    {
        return new RegionalEquityGrantSlot(Code, Label, Description, Quantity, StartsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
