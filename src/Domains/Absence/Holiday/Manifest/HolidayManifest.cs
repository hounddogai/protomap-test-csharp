namespace Acme.Workforce.Absence.Holiday.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday manifest is in its life cycle.</summary>
public enum HolidayManifestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One holiday manifest, with what the planners need to act on it.</summary>
public sealed class HolidayManifest(string code, string name, int capacity, string source, string ownerId, string region, double budget, int createdAt, double margin, int expiresAt, string badge, HolidayManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Source { get; set; } = source;
    public string OwnerId { get; set; } = ownerId;
    public string Region { get; set; } = region;
    public double Budget { get; set; } = budget;
    public int CreatedAt { get; set; } = createdAt;
    public double Margin { get; set; } = margin;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Badge { get; set; } = badge;
    public HolidayManifestStatus Status { get; set; } = status;

    /// <summary>Starts a holiday manifest in status draft.</summary>
    public static HolidayManifest NewHolidayManifest(string code, string name)
    {
        return new HolidayManifest(code, name, 0, "", "", "", 0.0, 0, 0.0, 0, "", HolidayManifestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayManifestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayManifestStatus next)
    {
        switch (Status)
        {
            case HolidayManifestStatus.Draft:
                return next == HolidayManifestStatus.Active || next == HolidayManifestStatus.Suspended;
            case HolidayManifestStatus.Pending:
                return next == HolidayManifestStatus.Suspended;
            case HolidayManifestStatus.Active:
                return next == HolidayManifestStatus.Closed;
            case HolidayManifestStatus.Suspended:
                switch (next)
                {
                    case HolidayManifestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"holiday manifest"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Capacity * Budget, 2);
    }

    public HolidayManifest Copy()
    {
        return new HolidayManifest(
            Code,
            Name,
            Capacity,
            Source,
            OwnerId,
            Region,
            Budget,
            CreatedAt,
            Margin,
            ExpiresAt,
            Badge,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
