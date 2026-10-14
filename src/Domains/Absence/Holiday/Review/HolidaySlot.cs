namespace Acme.Workforce.Absence.Holiday.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday slot is in its life cycle.</summary>
public enum HolidaySlotStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A holiday slot. Its code is unique within a tenant.</summary>
public sealed class HolidaySlot(string code, string title, int quantity, int version, int createdAt, int capacity, int attempts, string description, string region, Dictionary<string, string> attributes, string skill, HolidaySlotStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public int Version { get; set; } = version;
    public int CreatedAt { get; set; } = createdAt;
    public int Capacity { get; set; } = capacity;
    public int Attempts { get; set; } = attempts;
    public string Description { get; set; } = description;
    public string Region { get; set; } = region;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Skill { get; set; } = skill;
    public HolidaySlotStatus Status { get; set; } = status;

    /// <summary>Starts a holiday slot in status requested.</summary>
    public static HolidaySlot NewHolidaySlot(string code, string title)
    {
        return new HolidaySlot(
            code,
            title,
            0,
            0,
            0,
            0,
            0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            HolidaySlotStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidaySlotStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidaySlotStatus next)
    {
        switch (Status)
        {
            case HolidaySlotStatus.Requested:
                switch (next)
                {
                    case HolidaySlotStatus.Granted:
                    case HolidaySlotStatus.Revoked:
                        return true;
                }
                return false;
            case HolidaySlotStatus.Granted:
                switch (next)
                {
                    case HolidaySlotStatus.Expired:
                        return true;
                }
                return false;
            case HolidaySlotStatus.Revoked:
                switch (next)
                {
                    case HolidaySlotStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidaySlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Quantity + (double)CreatedAt, 2);
    }

    public HolidaySlot Copy()
    {
        return new HolidaySlot(
            Code,
            Title,
            Quantity,
            Version,
            CreatedAt,
            Capacity,
            Attempts,
            Description,
            Region,
            Attributes,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
