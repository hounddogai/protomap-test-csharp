namespace Acme.Workforce.Compensation.MeritCycle.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary merit cycle window is in its life cycle.</summary>
public enum PrimaryMeritCycleWindowStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One primary merit cycle window, with what the planners need to act on it.</summary>
public sealed class PrimaryMeritCycleWindow(string code, string name, bool pinned, int version, double threshold, double cost, bool archived, int endsAt, int graceDays, bool approved, string notes, PrimaryMeritCycleWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int Version { get; set; } = version;
    public double Threshold { get; set; } = threshold;
    public double Cost { get; set; } = cost;
    public bool Archived { get; set; } = archived;
    public int EndsAt { get; set; } = endsAt;
    public int GraceDays { get; set; } = graceDays;
    public bool Approved { get; set; } = approved;
    public string Notes { get; set; } = notes;
    public PrimaryMeritCycleWindowStatus Status { get; set; } = status;

    /// <summary>Starts a primary merit cycle window in status new.</summary>
    public static PrimaryMeritCycleWindow NewPrimaryMeritCycleWindow(string code, string name)
    {
        return new PrimaryMeritCycleWindow(
            code,
            name,
            false,
            0,
            0.0,
            0.0,
            false,
            0,
            0,
            false,
            "",
            PrimaryMeritCycleWindowStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryMeritCycleWindowStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryMeritCycleWindowStatus next)
    {
        switch (Status)
        {
            case PrimaryMeritCycleWindowStatus.New:
                switch (next)
                {
                    case PrimaryMeritCycleWindowStatus.Queued:
                    case PrimaryMeritCycleWindowStatus.Failed:
                        return true;
                }
                return false;
            case PrimaryMeritCycleWindowStatus.Queued:
                switch (next)
                {
                    case PrimaryMeritCycleWindowStatus.Running:
                    case PrimaryMeritCycleWindowStatus.Done:
                        return true;
                }
                return false;
            case PrimaryMeritCycleWindowStatus.Running:
                switch (next)
                {
                    case PrimaryMeritCycleWindowStatus.Done:
                        return true;
                }
                return false;
            case PrimaryMeritCycleWindowStatus.Done:
                return next == PrimaryMeritCycleWindowStatus.Failed;
        }
        return false;
    }

    public void MoveTo(PrimaryMeritCycleWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary merit cycle window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"primary merit cycle window"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)GraceDays + Cost, 2);
    }

    public PrimaryMeritCycleWindow Copy()
    {
        return new PrimaryMeritCycleWindow(
            Code,
            Name,
            Pinned,
            Version,
            Threshold,
            Cost,
            Archived,
            EndsAt,
            GraceDays,
            Approved,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
