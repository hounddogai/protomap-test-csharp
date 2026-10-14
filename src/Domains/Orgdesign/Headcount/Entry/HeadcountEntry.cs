namespace Acme.Workforce.Orgdesign.Headcount.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount entry is in its life cycle.</summary>
public enum HeadcountEntryStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One headcount entry, with what the planners need to act on it.</summary>
public sealed class HeadcountEntry(string code, string label, string channel, int durationMinutes, string contactEmail, List<string> tags, string managerId, HeadcountEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public List<string> Tags { get; set; } = tags;
    public string ManagerId { get; set; } = managerId;
    public HeadcountEntryStatus Status { get; set; } = status;

    /// <summary>Starts a headcount entry in status planned.</summary>
    public static HeadcountEntry NewHeadcountEntry(string code, string label)
    {
        return new HeadcountEntry(code, label, "", 0, "", new List<string>(), "", HeadcountEntryStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountEntryStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountEntryStatus next)
    {
        switch (Status)
        {
            case HeadcountEntryStatus.Planned:
                return next == HeadcountEntryStatus.Scheduled || next == HeadcountEntryStatus.InProgress;
            case HeadcountEntryStatus.Scheduled:
                switch (next)
                {
                    case HeadcountEntryStatus.InProgress:
                    case HeadcountEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountEntryStatus.InProgress:
                switch (next)
                {
                    case HeadcountEntryStatus.Cancelled:
                        return true;
                }
                return false;
            case HeadcountEntryStatus.Completed:
                switch (next)
                {
                    case HeadcountEntryStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"headcount entry"} {Code}";
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

    public double MeasuredValue()
    {
        return (double)DurationMinutes;
    }

    public HeadcountEntry Copy()
    {
        return new HeadcountEntry(Code, Label, Channel, DurationMinutes, ContactEmail, Tags, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
