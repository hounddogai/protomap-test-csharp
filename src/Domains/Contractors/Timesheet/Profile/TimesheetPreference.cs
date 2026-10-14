namespace Acme.Workforce.Contractors.Timesheet.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a timesheet preference is in its life cycle.</summary>
public enum TimesheetPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A timesheet preference. Its code is unique within a tenant.</summary>
public sealed class TimesheetPreference(string code, string name, int priority, Dictionary<string, string> attributes, int batchSize, TimesheetPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int BatchSize { get; set; } = batchSize;
    public TimesheetPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a timesheet preference in status requested.</summary>
    public static TimesheetPreference NewTimesheetPreference(string code, string name)
    {
        return new TimesheetPreference(
            code,
            name,
            0,
            new Dictionary<string, string>(),
            0,
            TimesheetPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TimesheetPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TimesheetPreferenceStatus next)
    {
        switch (Status)
        {
            case TimesheetPreferenceStatus.Requested:
                return next == TimesheetPreferenceStatus.Revoked || next == TimesheetPreferenceStatus.Expired;
            case TimesheetPreferenceStatus.Granted:
                switch (next)
                {
                    case TimesheetPreferenceStatus.Revoked:
                        return true;
                }
                return false;
            case TimesheetPreferenceStatus.Revoked:
                switch (next)
                {
                    case TimesheetPreferenceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TimesheetPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"timesheet preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
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

    public double EffectiveValue()
    {
        return Math.Round((double)BatchSize * (double)Priority, 2);
    }

    public TimesheetPreference Copy()
    {
        return new TimesheetPreference(Code, Name, Priority, Attributes, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
