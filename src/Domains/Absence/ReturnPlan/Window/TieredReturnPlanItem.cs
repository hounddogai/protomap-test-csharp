namespace Acme.Workforce.Absence.ReturnPlan.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered return plan item is in its life cycle.</summary>
public enum TieredReturnPlanItemStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered return plan item as the return plan team keeps it.</summary>
public sealed class TieredReturnPlanItem(string code, string title, string region, string ownerId, int quantity, string externalRef, string location, TieredReturnPlanItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string OwnerId { get; set; } = ownerId;
    public int Quantity { get; set; } = quantity;
    public string ExternalRef { get; set; } = externalRef;
    public string Location { get; set; } = location;
    public TieredReturnPlanItemStatus Status { get; set; } = status;

    /// <summary>Starts a tiered return plan item in status draft.</summary>
    public static TieredReturnPlanItem NewTieredReturnPlanItem(string code, string title)
    {
        return new TieredReturnPlanItem(code, title, "", "", 0, "", "", TieredReturnPlanItemStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredReturnPlanItemStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredReturnPlanItemStatus next)
    {
        switch (Status)
        {
            case TieredReturnPlanItemStatus.Draft:
                switch (next)
                {
                    case TieredReturnPlanItemStatus.Pending:
                    case TieredReturnPlanItemStatus.Closed:
                        return true;
                }
                return false;
            case TieredReturnPlanItemStatus.Pending:
                switch (next)
                {
                    case TieredReturnPlanItemStatus.Active:
                    case TieredReturnPlanItemStatus.Closed:
                        return true;
                }
                return false;
            case TieredReturnPlanItemStatus.Active:
                return next == TieredReturnPlanItemStatus.Suspended || next == TieredReturnPlanItemStatus.Closed;
            case TieredReturnPlanItemStatus.Suspended:
                return next == TieredReturnPlanItemStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TieredReturnPlanItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered return plan item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public TieredReturnPlanItem Copy()
    {
        return new TieredReturnPlanItem(Code, Title, Region, OwnerId, Quantity, ExternalRef, Location, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
