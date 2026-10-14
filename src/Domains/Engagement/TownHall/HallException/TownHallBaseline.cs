namespace Acme.Workforce.Engagement.TownHall.HallException;

using Acme.Workforce.Kit.Text;

/// <summary>Where a town hall baseline is in its life cycle.</summary>
public enum TownHallBaselineStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A town hall baseline. Its code is unique within a tenant.</summary>
public sealed class TownHallBaseline(string code, string name, int leaveDays, bool archived, string source, double threshold, TownHallBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int LeaveDays { get; set; } = leaveDays;
    public bool Archived { get; set; } = archived;
    public string Source { get; set; } = source;
    public double Threshold { get; set; } = threshold;
    public TownHallBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a town hall baseline in status new.</summary>
    public static TownHallBaseline NewTownHallBaseline(string code, string name)
    {
        return new TownHallBaseline(code, name, 0, false, "", 0.0, TownHallBaselineStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TownHallBaselineStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TownHallBaselineStatus next)
    {
        switch (Status)
        {
            case TownHallBaselineStatus.New:
                switch (next)
                {
                    case TownHallBaselineStatus.Done:
                        return true;
                }
                return false;
            case TownHallBaselineStatus.Queued:
                switch (next)
                {
                    case TownHallBaselineStatus.Failed:
                        return true;
                }
                return false;
            case TownHallBaselineStatus.Running:
                switch (next)
                {
                    case TownHallBaselineStatus.Done:
                        return true;
                }
                return false;
            case TownHallBaselineStatus.Done:
                return next == TownHallBaselineStatus.Failed;
        }
        return false;
    }

    public void MoveTo(TownHallBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"town hall baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)LeaveDays * Threshold, 2);
    }

    public TownHallBaseline Copy()
    {
        return new TownHallBaseline(Code, Name, LeaveDays, Archived, Source, Threshold, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
