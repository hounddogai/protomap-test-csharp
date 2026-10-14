namespace Acme.Workforce.Engagement.TownHall.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall schedule is in its life cycle.</summary>
public enum TownHallScheduleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A town hall schedule as the town hall team keeps it.</summary>
public sealed class TownHallSchedule(string code, string label, double hours, int expiresAt, double floorValue, string contactEmail, string summary, string description, Dictionary<string, string> attributes, TownHallScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Hours { get; set; } = hours;
    public int ExpiresAt { get; set; } = expiresAt;
    public double FloorValue { get; set; } = floorValue;
    public string ContactEmail { get; set; } = contactEmail;
    public string Summary { get; set; } = summary;
    public string Description { get; set; } = description;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public TownHallScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a town hall schedule in status requested.</summary>
    public static TownHallSchedule NewTownHallSchedule(string code, string label)
    {
        return new TownHallSchedule(
            code,
            label,
            0.0,
            0,
            0.0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            TownHallScheduleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallScheduleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallScheduleStatus next)
    {
        switch (Status)
        {
            case TownHallScheduleStatus.Requested:
                return next == TownHallScheduleStatus.Expired;
            case TownHallScheduleStatus.Granted:
                switch (next)
                {
                    case TownHallScheduleStatus.Expired:
                        return true;
                }
                return false;
            case TownHallScheduleStatus.Revoked:
                switch (next)
                {
                    case TownHallScheduleStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TownHallScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
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

    public double EffectiveExposure()
    {
        return Math.Round(Hours * (double)ExpiresAt, 2);
    }

    public TownHallSchedule Copy()
    {
        return new TownHallSchedule(
            Code,
            Label,
            Hours,
            ExpiresAt,
            FloorValue,
            ContactEmail,
            Summary,
            Description,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
