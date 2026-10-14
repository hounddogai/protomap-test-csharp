namespace Acme.Workforce.Engagement.TownHall.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall manifest is in its life cycle.</summary>
public enum LegacyTownHallManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A legacy town hall manifest as the town hall team keeps it.</summary>
public sealed class LegacyTownHallManifest(string code, string name, int createdAt, int tenureMonths, string category, string jobCode, LegacyTownHallManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int CreatedAt { get; set; } = createdAt;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Category { get; set; } = category;
    public string JobCode { get; set; } = jobCode;
    public LegacyTownHallManifestStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall manifest in status planned.</summary>
    public static LegacyTownHallManifest NewLegacyTownHallManifest(string code, string name)
    {
        return new LegacyTownHallManifest(code, name, 0, 0, "", "", LegacyTownHallManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallManifestStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallManifestStatus.Planned:
                return next == LegacyTownHallManifestStatus.Completed || next == LegacyTownHallManifestStatus.Cancelled;
            case LegacyTownHallManifestStatus.Scheduled:
                return next == LegacyTownHallManifestStatus.Completed || next == LegacyTownHallManifestStatus.Cancelled;
            case LegacyTownHallManifestStatus.InProgress:
                switch (next)
                {
                    case LegacyTownHallManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case LegacyTownHallManifestStatus.Completed:
                switch (next)
                {
                    case LegacyTownHallManifestStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double NetVolume()
    {
        return Math.Round((double)TenureMonths - (double)CreatedAt, 2);
    }

    public LegacyTownHallManifest Copy()
    {
        return new LegacyTownHallManifest(Code, Name, CreatedAt, TenureMonths, Category, JobCode, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
