namespace Acme.Workforce.Orgdesign.Reorg.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg group is in its life cycle.</summary>
public enum ReorgGroupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One reorg group, with what the planners need to act on it.</summary>
public sealed class ReorgGroup(string code, string title, int tenureMonths, int capacity, double ceiling, string team, double maximum, string unit, ReorgGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Capacity { get; set; } = capacity;
    public double Ceiling { get; set; } = ceiling;
    public string Team { get; set; } = team;
    public double Maximum { get; set; } = maximum;
    public string Unit { get; set; } = unit;
    public ReorgGroupStatus Status { get; set; } = status;

    /// <summary>Starts a reorg group in status idle.</summary>
    public static ReorgGroup NewReorgGroup(string code, string title)
    {
        return new ReorgGroup(code, title, 0, 0, 0.0, "", 0.0, "", ReorgGroupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgGroupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgGroupStatus next)
    {
        switch (Status)
        {
            case ReorgGroupStatus.Idle:
                return next == ReorgGroupStatus.Draining;
            case ReorgGroupStatus.Warming:
                return next == ReorgGroupStatus.Ready || next == ReorgGroupStatus.Draining;
            case ReorgGroupStatus.Ready:
                return next == ReorgGroupStatus.Stopped;
            case ReorgGroupStatus.Draining:
                switch (next)
                {
                    case ReorgGroupStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"reorg group"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Ceiling * (double)Capacity, 2);
    }

    public ReorgGroup Copy()
    {
        return new ReorgGroup(Code, Title, TenureMonths, Capacity, Ceiling, Team, Maximum, Unit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
