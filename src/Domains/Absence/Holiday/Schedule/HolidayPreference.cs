namespace Acme.Workforce.Absence.Holiday.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a holiday preference is in its life cycle.</summary>
public enum HolidayPreferenceStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A holiday preference as the holiday team keeps it.</summary>
public sealed class HolidayPreference(string code, string title, double minimum, string phone, int capacity, Dictionary<string, string> attributes, string badge, HolidayPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Phone { get; set; } = phone;
    public int Capacity { get; set; } = capacity;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Badge { get; set; } = badge;
    public HolidayPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a holiday preference in status idle.</summary>
    public static HolidayPreference NewHolidayPreference(string code, string title)
    {
        return new HolidayPreference(
            code,
            title,
            0.0,
            "",
            0,
            new Dictionary<string, string>(),
            "",
            HolidayPreferenceStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HolidayPreferenceStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HolidayPreferenceStatus next)
    {
        switch (Status)
        {
            case HolidayPreferenceStatus.Idle:
                switch (next)
                {
                    case HolidayPreferenceStatus.Ready:
                    case HolidayPreferenceStatus.Draining:
                        return true;
                }
                return false;
            case HolidayPreferenceStatus.Warming:
                switch (next)
                {
                    case HolidayPreferenceStatus.Stopped:
                        return true;
                }
                return false;
            case HolidayPreferenceStatus.Ready:
                return next == HolidayPreferenceStatus.Stopped;
            case HolidayPreferenceStatus.Draining:
                switch (next)
                {
                    case HolidayPreferenceStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HolidayPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"holiday preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double ProjectedLoad()
    {
        return Math.Round(Minimum * (double)Capacity, 2);
    }

    public HolidayPreference Copy()
    {
        return new HolidayPreference(Code, Title, Minimum, Phone, Capacity, Attributes, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
