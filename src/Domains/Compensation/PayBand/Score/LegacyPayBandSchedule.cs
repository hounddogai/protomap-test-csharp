namespace Acme.Workforce.Compensation.PayBand.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy pay band schedule is in its life cycle.</summary>
public enum LegacyPayBandScheduleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A legacy pay band schedule as the pay band team keeps it.</summary>
public sealed class LegacyPayBandSchedule(string code, string name, int updatedAt, string currency, string timezone, string country, Dictionary<string, string> attributes, string title, LegacyPayBandScheduleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Currency { get; set; } = currency;
    public string Timezone { get; set; } = timezone;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Title { get; set; } = title;
    public LegacyPayBandScheduleStatus Status { get; set; } = status;

    /// <summary>Starts a legacy pay band schedule in status idle.</summary>
    public static LegacyPayBandSchedule NewLegacyPayBandSchedule(string code, string name)
    {
        return new LegacyPayBandSchedule(
            code,
            name,
            0,
            "",
            "",
            "",
            new Dictionary<string, string>(),
            "",
            LegacyPayBandScheduleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyPayBandScheduleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyPayBandScheduleStatus next)
    {
        switch (Status)
        {
            case LegacyPayBandScheduleStatus.Idle:
                return next == LegacyPayBandScheduleStatus.Warming || next == LegacyPayBandScheduleStatus.Draining;
            case LegacyPayBandScheduleStatus.Warming:
                return next == LegacyPayBandScheduleStatus.Draining || next == LegacyPayBandScheduleStatus.Stopped;
            case LegacyPayBandScheduleStatus.Ready:
                switch (next)
                {
                    case LegacyPayBandScheduleStatus.Draining:
                        return true;
                }
                return false;
            case LegacyPayBandScheduleStatus.Draining:
                switch (next)
                {
                    case LegacyPayBandScheduleStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyPayBandScheduleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy pay band schedule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
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

    public double MeasuredValue()
    {
        return (double)UpdatedAt;
    }

    public LegacyPayBandSchedule Copy()
    {
        return new LegacyPayBandSchedule(
            Code,
            Name,
            UpdatedAt,
            Currency,
            Timezone,
            Country,
            Attributes,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
