namespace Acme.Workforce.Absence.Balance.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional balance digest is in its life cycle.</summary>
public enum RegionalBalanceDigestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional balance digest. Its code is unique within a tenant.</summary>
public sealed class RegionalBalanceDigest(string code, string name, string team, int endsAt, int leaveDays, bool visible, string skill, double fte, string costCenter, double cost, string locale, RegionalBalanceDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Team { get; set; } = team;
    public int EndsAt { get; set; } = endsAt;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Visible { get; set; } = visible;
    public string Skill { get; set; } = skill;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public double Cost { get; set; } = cost;
    public string Locale { get; set; } = locale;
    public RegionalBalanceDigestStatus Status { get; set; } = status;

    /// <summary>Starts a regional balance digest in status planned.</summary>
    public static RegionalBalanceDigest NewRegionalBalanceDigest(string code, string name)
    {
        return new RegionalBalanceDigest(
            code,
            name,
            "",
            0,
            0,
            false,
            "",
            0.0,
            "",
            0.0,
            "",
            RegionalBalanceDigestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBalanceDigestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBalanceDigestStatus next)
    {
        switch (Status)
        {
            case RegionalBalanceDigestStatus.Planned:
                return next == RegionalBalanceDigestStatus.InProgress;
            case RegionalBalanceDigestStatus.Scheduled:
                return next == RegionalBalanceDigestStatus.Cancelled;
            case RegionalBalanceDigestStatus.InProgress:
                return next == RegionalBalanceDigestStatus.Completed || next == RegionalBalanceDigestStatus.Cancelled;
            case RegionalBalanceDigestStatus.Completed:
                return next == RegionalBalanceDigestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(RegionalBalanceDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional balance digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte - Cost, 2);
    }

    public RegionalBalanceDigest Copy()
    {
        return new RegionalBalanceDigest(
            Code,
            Name,
            Team,
            EndsAt,
            LeaveDays,
            Visible,
            Skill,
            Fte,
            CostCenter,
            Cost,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
