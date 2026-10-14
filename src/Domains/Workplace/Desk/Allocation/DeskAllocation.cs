namespace Acme.Workforce.Workplace.Desk.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk allocation is in its life cycle.</summary>
public enum DeskAllocationStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A desk allocation. Its code is unique within a tenant.</summary>
public sealed class DeskAllocation(string code, string title, string channel, string location, double fte, string timezone, bool visible, double margin, double amount, string description, string costCenter, DeskAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Location { get; set; } = location;
    public double Fte { get; set; } = fte;
    public string Timezone { get; set; } = timezone;
    public bool Visible { get; set; } = visible;
    public double Margin { get; set; } = margin;
    public double Amount { get; set; } = amount;
    public string Description { get; set; } = description;
    public string CostCenter { get; set; } = costCenter;
    public DeskAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a desk allocation in status created.</summary>
    public static DeskAllocation NewDeskAllocation(string code, string title)
    {
        return new DeskAllocation(
            code,
            title,
            "",
            "",
            0.0,
            "",
            false,
            0.0,
            0.0,
            "",
            "",
            DeskAllocationStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskAllocationStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskAllocationStatus next)
    {
        switch (Status)
        {
            case DeskAllocationStatus.Created:
                switch (next)
                {
                    case DeskAllocationStatus.Published:
                    case DeskAllocationStatus.Retired:
                        return true;
                }
                return false;
            case DeskAllocationStatus.Validated:
                return next == DeskAllocationStatus.Published;
            case DeskAllocationStatus.Published:
                return next == DeskAllocationStatus.Retired;
        }
        return false;
    }

    public void MoveTo(DeskAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"desk allocation"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Margin + Fte, 2);
    }

    public DeskAllocation Copy()
    {
        return new DeskAllocation(
            Code,
            Title,
            Channel,
            Location,
            Fte,
            Timezone,
            Visible,
            Margin,
            Amount,
            Description,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
