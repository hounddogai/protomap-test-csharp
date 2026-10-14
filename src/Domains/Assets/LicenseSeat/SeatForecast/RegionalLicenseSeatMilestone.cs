namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional license seat milestone is in its life cycle.</summary>
public enum RegionalLicenseSeatMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A regional license seat milestone as the license seat team keeps it.</summary>
public sealed class RegionalLicenseSeatMilestone(string code, string title, double cost, bool pinned, string employeeId, string notes, string managerId, double discount, int capacity, int version, string skill, RegionalLicenseSeatMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public bool Pinned { get; set; } = pinned;
    public string EmployeeId { get; set; } = employeeId;
    public string Notes { get; set; } = notes;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public int Capacity { get; set; } = capacity;
    public int Version { get; set; } = version;
    public string Skill { get; set; } = skill;
    public RegionalLicenseSeatMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a regional license seat milestone in status draft.</summary>
    public static RegionalLicenseSeatMilestone NewRegionalLicenseSeatMilestone(string code, string title)
    {
        return new RegionalLicenseSeatMilestone(
            code,
            title,
            0.0,
            false,
            "",
            "",
            "",
            0.0,
            0,
            0,
            "",
            RegionalLicenseSeatMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalLicenseSeatMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalLicenseSeatMilestoneStatus next)
    {
        switch (Status)
        {
            case RegionalLicenseSeatMilestoneStatus.Draft:
                return next == RegionalLicenseSeatMilestoneStatus.Suspended || next == RegionalLicenseSeatMilestoneStatus.Closed;
            case RegionalLicenseSeatMilestoneStatus.Pending:
                switch (next)
                {
                    case RegionalLicenseSeatMilestoneStatus.Active:
                    case RegionalLicenseSeatMilestoneStatus.Suspended:
                        return true;
                }
                return false;
            case RegionalLicenseSeatMilestoneStatus.Active:
                switch (next)
                {
                    case RegionalLicenseSeatMilestoneStatus.Closed:
                        return true;
                }
                return false;
            case RegionalLicenseSeatMilestoneStatus.Suspended:
                switch (next)
                {
                    case RegionalLicenseSeatMilestoneStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalLicenseSeatMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional license seat milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)Version + Discount, 2);
    }

    public RegionalLicenseSeatMilestone Copy()
    {
        return new RegionalLicenseSeatMilestone(
            Code,
            Title,
            Cost,
            Pinned,
            EmployeeId,
            Notes,
            ManagerId,
            Discount,
            Capacity,
            Version,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
