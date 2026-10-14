namespace Acme.Workforce.Workplace.Desk.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk tier is in its life cycle.</summary>
public enum DeskTierStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a desk tier and its current status.</summary>
public sealed class DeskTier(string code, string name, double floorValue, int createdAt, double threshold, string team, string jobCode, string costCenter, string description, DeskTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public int CreatedAt { get; set; } = createdAt;
    public double Threshold { get; set; } = threshold;
    public string Team { get; set; } = team;
    public string JobCode { get; set; } = jobCode;
    public string CostCenter { get; set; } = costCenter;
    public string Description { get; set; } = description;
    public DeskTierStatus Status { get; set; } = status;

    /// <summary>Starts a desk tier in status proposed.</summary>
    public static DeskTier NewDeskTier(string code, string name)
    {
        return new DeskTier(code, name, 0.0, 0, 0.0, "", "", "", "", DeskTierStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskTierStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskTierStatus next)
    {
        switch (Status)
        {
            case DeskTierStatus.Proposed:
                return next == DeskTierStatus.Rejected;
            case DeskTierStatus.Approved:
                switch (next)
                {
                    case DeskTierStatus.Rejected:
                    case DeskTierStatus.Withdrawn:
                        return true;
                }
                return false;
            case DeskTierStatus.Rejected:
                switch (next)
                {
                    case DeskTierStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"desk tier"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Threshold * FloorValue, 2);
    }

    public DeskTier Copy()
    {
        return new DeskTier(
            Code,
            Name,
            FloorValue,
            CreatedAt,
            Threshold,
            Team,
            JobCode,
            CostCenter,
            Description,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
