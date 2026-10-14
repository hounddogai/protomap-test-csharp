namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk rollup is in its life cycle.</summary>
public enum DeskRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One desk rollup, with what the planners need to act on it.</summary>
public sealed class DeskRollup(string code, string name, double rate, string skill, int batchSize, DeskRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Skill { get; set; } = skill;
    public int BatchSize { get; set; } = batchSize;
    public DeskRollupStatus Status { get; set; } = status;

    /// <summary>Starts a desk rollup in status idle.</summary>
    public static DeskRollup NewDeskRollup(string code, string name)
    {
        return new DeskRollup(code, name, 0.0, "", 0, DeskRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskRollupStatus next)
    {
        switch (Status)
        {
            case DeskRollupStatus.Idle:
                switch (next)
                {
                    case DeskRollupStatus.Ready:
                    case DeskRollupStatus.Draining:
                        return true;
                }
                return false;
            case DeskRollupStatus.Warming:
                return next == DeskRollupStatus.Draining;
            case DeskRollupStatus.Ready:
                return next == DeskRollupStatus.Draining || next == DeskRollupStatus.Stopped;
            case DeskRollupStatus.Draining:
                switch (next)
                {
                    case DeskRollupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)BatchSize - Rate, 2);
    }

    public DeskRollup Copy()
    {
        return new DeskRollup(Code, Name, Rate, Skill, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
