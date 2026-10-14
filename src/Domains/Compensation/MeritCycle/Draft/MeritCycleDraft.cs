namespace Acme.Workforce.Compensation.MeritCycle.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle draft is in its life cycle.</summary>
public enum MeritCycleDraftStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One merit cycle draft, with what the planners need to act on it.</summary>
public sealed class MeritCycleDraft(string code, string name, string title, bool visible, string location, string team, int version, string region, MeritCycleDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public bool Visible { get; set; } = visible;
    public string Location { get; set; } = location;
    public string Team { get; set; } = team;
    public int Version { get; set; } = version;
    public string Region { get; set; } = region;
    public MeritCycleDraftStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle draft in status new.</summary>
    public static MeritCycleDraft NewMeritCycleDraft(string code, string name)
    {
        return new MeritCycleDraft(code, name, "", false, "", "", 0, "", MeritCycleDraftStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleDraftStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleDraftStatus next)
    {
        switch (Status)
        {
            case MeritCycleDraftStatus.New:
                switch (next)
                {
                    case MeritCycleDraftStatus.Running:
                    case MeritCycleDraftStatus.Done:
                        return true;
                }
                return false;
            case MeritCycleDraftStatus.Queued:
                switch (next)
                {
                    case MeritCycleDraftStatus.Running:
                        return true;
                }
                return false;
            case MeritCycleDraftStatus.Running:
                switch (next)
                {
                    case MeritCycleDraftStatus.Done:
                    case MeritCycleDraftStatus.Failed:
                        return true;
                }
                return false;
            case MeritCycleDraftStatus.Done:
                return next == MeritCycleDraftStatus.Failed;
        }
        return false;
    }

    public void MoveTo(MeritCycleDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Version;
    }

    public MeritCycleDraft Copy()
    {
        return new MeritCycleDraft(Code, Name, Title, Visible, Location, Team, Version, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
