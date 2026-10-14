namespace Acme.Workforce.Compensation.Adjustment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adjustment item is in its life cycle.</summary>
public enum AdjustmentItemStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A adjustment item as the adjustment team keeps it.</summary>
public sealed class AdjustmentItem(string code, string title, string name, string region, int updatedAt, double margin, double budget, AdjustmentItemStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Margin { get; set; } = margin;
    public double Budget { get; set; } = budget;
    public AdjustmentItemStatus Status { get; set; } = status;

    /// <summary>Starts a adjustment item in status draft.</summary>
    public static AdjustmentItem NewAdjustmentItem(string code, string title)
    {
        return new AdjustmentItem(code, title, "", "", 0, 0.0, 0.0, AdjustmentItemStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdjustmentItemStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdjustmentItemStatus next)
    {
        switch (Status)
        {
            case AdjustmentItemStatus.Draft:
                return next == AdjustmentItemStatus.Active || next == AdjustmentItemStatus.Suspended;
            case AdjustmentItemStatus.Pending:
                switch (next)
                {
                    case AdjustmentItemStatus.Active:
                        return true;
                }
                return false;
            case AdjustmentItemStatus.Active:
                switch (next)
                {
                    case AdjustmentItemStatus.Suspended:
                        return true;
                }
                return false;
            case AdjustmentItemStatus.Suspended:
                return next == AdjustmentItemStatus.Closed;
        }
        return false;
    }

    public void MoveTo(AdjustmentItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adjustment item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetLoad()
    {
        return Math.Round((double)UpdatedAt - Budget, 2);
    }

    public AdjustmentItem Copy()
    {
        return new AdjustmentItem(Code, Title, Name, Region, UpdatedAt, Margin, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
