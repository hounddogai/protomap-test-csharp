namespace Acme.Workforce.Talent.Mobility.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled mobility rollup is in its life cycle.</summary>
public enum ScheduledMobilityRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One scheduled mobility rollup, with what the planners need to act on it.</summary>
public sealed class ScheduledMobilityRollup(string code, string title, bool approved, bool visible, string phone, string managerId, bool pinned, int quantity, ScheduledMobilityRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Approved { get; set; } = approved;
    public bool Visible { get; set; } = visible;
    public string Phone { get; set; } = phone;
    public string ManagerId { get; set; } = managerId;
    public bool Pinned { get; set; } = pinned;
    public int Quantity { get; set; } = quantity;
    public ScheduledMobilityRollupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled mobility rollup in status new.</summary>
    public static ScheduledMobilityRollup NewScheduledMobilityRollup(string code, string title)
    {
        return new ScheduledMobilityRollup(
            code,
            title,
            false,
            false,
            "",
            "",
            false,
            0,
            ScheduledMobilityRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMobilityRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMobilityRollupStatus next)
    {
        switch (Status)
        {
            case ScheduledMobilityRollupStatus.New:
                return next == ScheduledMobilityRollupStatus.Failed;
            case ScheduledMobilityRollupStatus.Queued:
                switch (next)
                {
                    case ScheduledMobilityRollupStatus.Running:
                    case ScheduledMobilityRollupStatus.Done:
                        return true;
                }
                return false;
            case ScheduledMobilityRollupStatus.Running:
                return next == ScheduledMobilityRollupStatus.Done || next == ScheduledMobilityRollupStatus.Failed;
            case ScheduledMobilityRollupStatus.Done:
                switch (next)
                {
                    case ScheduledMobilityRollupStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledMobilityRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled mobility rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled mobility rollup"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public ScheduledMobilityRollup Copy()
    {
        return new ScheduledMobilityRollup(
            Code,
            Title,
            Approved,
            Visible,
            Phone,
            ManagerId,
            Pinned,
            Quantity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
