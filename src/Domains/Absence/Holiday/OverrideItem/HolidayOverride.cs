namespace Acme.Workforce.Absence.Holiday.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday override is in its life cycle.</summary>
public enum HolidayOverrideStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One holiday override, with what the planners need to act on it.</summary>
public sealed class HolidayOverride(string code, string title, string summary, string checksum, string channel, List<string> tags, bool approved, Dictionary<string, string> attributes, string phone, double score, HolidayOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string Checksum { get; set; } = checksum;
    public string Channel { get; set; } = channel;
    public List<string> Tags { get; set; } = tags;
    public bool Approved { get; set; } = approved;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Phone { get; set; } = phone;
    public double Score { get; set; } = score;
    public HolidayOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a holiday override in status idle.</summary>
    public static HolidayOverride NewHolidayOverride(string code, string title)
    {
        return new HolidayOverride(
            code,
            title,
            "",
            "",
            "",
            new List<string>(),
            false,
            new Dictionary<string, string>(),
            "",
            0.0,
            HolidayOverrideStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayOverrideStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayOverrideStatus next)
    {
        switch (Status)
        {
            case HolidayOverrideStatus.Idle:
                return next == HolidayOverrideStatus.Draining || next == HolidayOverrideStatus.Stopped;
            case HolidayOverrideStatus.Warming:
                return next == HolidayOverrideStatus.Draining || next == HolidayOverrideStatus.Stopped;
            case HolidayOverrideStatus.Ready:
                return next == HolidayOverrideStatus.Stopped;
            case HolidayOverrideStatus.Draining:
                switch (next)
                {
                    case HolidayOverrideStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
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
        return Score;
    }

    public HolidayOverride Copy()
    {
        return new HolidayOverride(
            Code,
            Title,
            Summary,
            Checksum,
            Channel,
            Tags,
            Approved,
            Attributes,
            Phone,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
