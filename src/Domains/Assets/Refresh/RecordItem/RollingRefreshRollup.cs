namespace Acme.Workforce.Assets.Refresh.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling refresh rollup is in its life cycle.</summary>
public enum RollingRefreshRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One rolling refresh rollup, with what the planners need to act on it.</summary>
public sealed class RollingRefreshRollup(string code, string title, string unit, double ratio, List<string> tags, RollingRefreshRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Unit { get; set; } = unit;
    public double Ratio { get; set; } = ratio;
    public List<string> Tags { get; set; } = tags;
    public RollingRefreshRollupStatus Status { get; set; } = status;

    /// <summary>Starts a rolling refresh rollup in status new.</summary>
    public static RollingRefreshRollup NewRollingRefreshRollup(string code, string title)
    {
        return new RollingRefreshRollup(code, title, "", 0.0, new List<string>(), RollingRefreshRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingRefreshRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingRefreshRollupStatus next)
    {
        switch (Status)
        {
            case RollingRefreshRollupStatus.New:
                return next == RollingRefreshRollupStatus.Running || next == RollingRefreshRollupStatus.Failed;
            case RollingRefreshRollupStatus.Queued:
                return next == RollingRefreshRollupStatus.Failed;
            case RollingRefreshRollupStatus.Running:
                switch (next)
                {
                    case RollingRefreshRollupStatus.Failed:
                        return true;
                }
                return false;
            case RollingRefreshRollupStatus.Done:
                switch (next)
                {
                    case RollingRefreshRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingRefreshRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling refresh rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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
        return Ratio;
    }

    public RollingRefreshRollup Copy()
    {
        return new RollingRefreshRollup(Code, Title, Unit, Ratio, Tags, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
