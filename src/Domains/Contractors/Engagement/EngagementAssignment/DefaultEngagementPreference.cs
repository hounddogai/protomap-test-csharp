namespace Acme.Workforce.Contractors.Engagement.EngagementAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default engagement preference is in its life cycle.</summary>
public enum DefaultEngagementPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default engagement preference as the engagement team keeps it.</summary>
public sealed class DefaultEngagementPreference(string code, string name, string costCenter, string label, bool pinned, int limit, DefaultEngagementPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string CostCenter { get; set; } = costCenter;
    public string Label { get; set; } = label;
    public bool Pinned { get; set; } = pinned;
    public int Limit { get; set; } = limit;
    public DefaultEngagementPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a default engagement preference in status planned.</summary>
    public static DefaultEngagementPreference NewDefaultEngagementPreference(string code, string name)
    {
        return new DefaultEngagementPreference(
            code,
            name,
            "",
            "",
            false,
            0,
            DefaultEngagementPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultEngagementPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultEngagementPreferenceStatus next)
    {
        switch (Status)
        {
            case DefaultEngagementPreferenceStatus.Planned:
                switch (next)
                {
                    case DefaultEngagementPreferenceStatus.InProgress:
                    case DefaultEngagementPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultEngagementPreferenceStatus.Scheduled:
                switch (next)
                {
                    case DefaultEngagementPreferenceStatus.InProgress:
                    case DefaultEngagementPreferenceStatus.Completed:
                        return true;
                }
                return false;
            case DefaultEngagementPreferenceStatus.InProgress:
                switch (next)
                {
                    case DefaultEngagementPreferenceStatus.Completed:
                    case DefaultEngagementPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultEngagementPreferenceStatus.Completed:
                switch (next)
                {
                    case DefaultEngagementPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultEngagementPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default engagement preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public DefaultEngagementPreference Copy()
    {
        return new DefaultEngagementPreference(Code, Name, CostCenter, Label, Pinned, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
