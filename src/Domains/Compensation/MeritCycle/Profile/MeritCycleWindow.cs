namespace Acme.Workforce.Compensation.MeritCycle.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle window is in its life cycle.</summary>
public enum MeritCycleWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A merit cycle window as the merit cycle team keeps it.</summary>
public sealed class MeritCycleWindow(string code, string name, string title, double fte, bool archived, MeritCycleWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public bool Archived { get; set; } = archived;
    public MeritCycleWindowStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle window in status new.</summary>
    public static MeritCycleWindow NewMeritCycleWindow(string code, string name)
    {
        return new MeritCycleWindow(code, name, "", 0.0, false, MeritCycleWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleWindowStatus next)
    {
        switch (Status)
        {
            case MeritCycleWindowStatus.New:
                return next == MeritCycleWindowStatus.Queued || next == MeritCycleWindowStatus.Done;
            case MeritCycleWindowStatus.Queued:
                switch (next)
                {
                    case MeritCycleWindowStatus.Running:
                    case MeritCycleWindowStatus.Failed:
                        return true;
                }
                return false;
            case MeritCycleWindowStatus.Running:
                return next == MeritCycleWindowStatus.Failed;
            case MeritCycleWindowStatus.Done:
                return next == MeritCycleWindowStatus.Failed;
        }
        return false;
    }

    public void MoveTo(MeritCycleWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double MeasuredValue()
    {
        return Fte;
    }

    public MeritCycleWindow Copy()
    {
        return new MeritCycleWindow(Code, Name, Title, Fte, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
