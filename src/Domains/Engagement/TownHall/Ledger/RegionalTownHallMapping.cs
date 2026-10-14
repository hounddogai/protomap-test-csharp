namespace Acme.Workforce.Engagement.TownHall.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional town hall mapping is in its life cycle.</summary>
public enum RegionalTownHallMappingStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A regional town hall mapping as the town hall team keeps it.</summary>
public sealed class RegionalTownHallMapping(string code, string name, int version, string reviewer, int leaveDays, double score, int startsAt, double budget, RegionalTownHallMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public string Reviewer { get; set; } = reviewer;
    public int LeaveDays { get; set; } = leaveDays;
    public double Score { get; set; } = score;
    public int StartsAt { get; set; } = startsAt;
    public double Budget { get; set; } = budget;
    public RegionalTownHallMappingStatus Status { get; set; } = status;

    /// <summary>Starts a regional town hall mapping in status new.</summary>
    public static RegionalTownHallMapping NewRegionalTownHallMapping(string code, string name)
    {
        return new RegionalTownHallMapping(code, name, 0, "", 0, 0.0, 0, 0.0, RegionalTownHallMappingStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalTownHallMappingStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalTownHallMappingStatus next)
    {
        switch (Status)
        {
            case RegionalTownHallMappingStatus.New:
                switch (next)
                {
                    case RegionalTownHallMappingStatus.Done:
                        return true;
                }
                return false;
            case RegionalTownHallMappingStatus.Queued:
                switch (next)
                {
                    case RegionalTownHallMappingStatus.Done:
                    case RegionalTownHallMappingStatus.Failed:
                        return true;
                }
                return false;
            case RegionalTownHallMappingStatus.Running:
                switch (next)
                {
                    case RegionalTownHallMappingStatus.Failed:
                        return true;
                }
                return false;
            case RegionalTownHallMappingStatus.Done:
                return next == RegionalTownHallMappingStatus.Failed;
        }
        return false;
    }

    public void MoveTo(RegionalTownHallMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional town hall mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)StartsAt - (double)Version, 2);
    }

    public RegionalTownHallMapping Copy()
    {
        return new RegionalTownHallMapping(
            Code,
            Name,
            Version,
            Reviewer,
            LeaveDays,
            Score,
            StartsAt,
            Budget,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
