namespace Acme.Workforce.Workplace.Visitor.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a visitor entry is in its life cycle.</summary>
public enum VisitorEntryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A visitor entry as the visitor team keeps it.</summary>
public sealed class VisitorEntry(string code, string title, int expiresAt, int revision, string location, int createdAt, int windowDays, double margin, string channel, string timezone, VisitorEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Revision { get; set; } = revision;
    public string Location { get; set; } = location;
    public int CreatedAt { get; set; } = createdAt;
    public int WindowDays { get; set; } = windowDays;
    public double Margin { get; set; } = margin;
    public string Channel { get; set; } = channel;
    public string Timezone { get; set; } = timezone;
    public VisitorEntryStatus Status { get; set; } = status;

    /// <summary>Starts a visitor entry in status new.</summary>
    public static VisitorEntry NewVisitorEntry(string code, string title)
    {
        return new VisitorEntry(code, title, 0, 0, "", 0, 0, 0.0, "", "", VisitorEntryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case VisitorEntryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(VisitorEntryStatus next)
    {
        switch (Status)
        {
            case VisitorEntryStatus.New:
                switch (next)
                {
                    case VisitorEntryStatus.Failed:
                        return true;
                }
                return false;
            case VisitorEntryStatus.Queued:
                return next == VisitorEntryStatus.Done || next == VisitorEntryStatus.Failed;
            case VisitorEntryStatus.Running:
                return next == VisitorEntryStatus.Done || next == VisitorEntryStatus.Failed;
            case VisitorEntryStatus.Done:
                return next == VisitorEntryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(VisitorEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"visitor entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"visitor entry"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt * (double)Revision, 2);
    }

    public VisitorEntry Copy()
    {
        return new VisitorEntry(
            Code,
            Title,
            ExpiresAt,
            Revision,
            Location,
            CreatedAt,
            WindowDays,
            Margin,
            Channel,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
