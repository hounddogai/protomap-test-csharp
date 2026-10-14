namespace Acme.Workforce.Payrollops.RetroPay.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual retro pay tier is in its life cycle.</summary>
public enum ManualRetroPayTierStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One manual retro pay tier, with what the planners need to act on it.</summary>
public sealed class ManualRetroPayTier(string code, string name, double floorValue, string team, string managerId, int version, ManualRetroPayTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Team { get; set; } = team;
    public string ManagerId { get; set; } = managerId;
    public int Version { get; set; } = version;
    public ManualRetroPayTierStatus Status { get; set; } = status;

    /// <summary>Starts a manual retro pay tier in status open.</summary>
    public static ManualRetroPayTier NewManualRetroPayTier(string code, string name)
    {
        return new ManualRetroPayTier(code, name, 0.0, "", "", 0, ManualRetroPayTierStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRetroPayTierStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRetroPayTierStatus next)
    {
        switch (Status)
        {
            case ManualRetroPayTierStatus.Open:
                return next == ManualRetroPayTierStatus.Resolved || next == ManualRetroPayTierStatus.Reopened;
            case ManualRetroPayTierStatus.InReview:
                return next == ManualRetroPayTierStatus.Resolved || next == ManualRetroPayTierStatus.Reopened;
            case ManualRetroPayTierStatus.Resolved:
                switch (next)
                {
                    case ManualRetroPayTierStatus.Reopened:
                        return true;
                }
                return false;
            case ManualRetroPayTierStatus.Reopened:
                return next == ManualRetroPayTierStatus.Archived;
        }
        return false;
    }

    public void MoveTo(ManualRetroPayTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual retro pay tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetExposure()
    {
        return Math.Round((double)Version - FloorValue, 2);
    }

    public ManualRetroPayTier Copy()
    {
        return new ManualRetroPayTier(Code, Name, FloorValue, Team, ManagerId, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
