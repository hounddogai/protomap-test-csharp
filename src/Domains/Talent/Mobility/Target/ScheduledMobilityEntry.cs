namespace Acme.Workforce.Talent.Mobility.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled mobility entry is in its life cycle.</summary>
public enum ScheduledMobilityEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scheduled mobility entry, with what the planners need to act on it.</summary>
public sealed class ScheduledMobilityEntry(string code, string title, double ratio, string unit, string location, string managerId, string channel, Dictionary<string, string> attributes, double maximum, string contactEmail, ScheduledMobilityEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public string Unit { get; set; } = unit;
    public string Location { get; set; } = location;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Maximum { get; set; } = maximum;
    public string ContactEmail { get; set; } = contactEmail;
    public ScheduledMobilityEntryStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled mobility entry in status new.</summary>
    public static ScheduledMobilityEntry NewScheduledMobilityEntry(string code, string title)
    {
        return new ScheduledMobilityEntry(
            code,
            title,
            0.0,
            "",
            "",
            "",
            "",
            new Dictionary<string, string>(),
            0.0,
            "",
            ScheduledMobilityEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMobilityEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMobilityEntryStatus next)
    {
        switch (Status)
        {
            case ScheduledMobilityEntryStatus.New:
                return next == ScheduledMobilityEntryStatus.Failed;
            case ScheduledMobilityEntryStatus.Queued:
                return next == ScheduledMobilityEntryStatus.Running || next == ScheduledMobilityEntryStatus.Done;
            case ScheduledMobilityEntryStatus.Running:
                switch (next)
                {
                    case ScheduledMobilityEntryStatus.Done:
                        return true;
                }
                return false;
            case ScheduledMobilityEntryStatus.Done:
                return next == ScheduledMobilityEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledMobilityEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled mobility entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
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

    public double NetLoad()
    {
        return Math.Round(Ratio + Maximum, 2);
    }

    public ScheduledMobilityEntry Copy()
    {
        return new ScheduledMobilityEntry(
            Code,
            Title,
            Ratio,
            Unit,
            Location,
            ManagerId,
            Channel,
            Attributes,
            Maximum,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
