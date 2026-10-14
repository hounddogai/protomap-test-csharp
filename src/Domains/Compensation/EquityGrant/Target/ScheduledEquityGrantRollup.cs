namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled equity grant rollup is in its life cycle.</summary>
public enum ScheduledEquityGrantRollupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled equity grant rollup. Its code is unique within a tenant.</summary>
public sealed class ScheduledEquityGrantRollup(string code, string label, int quantity, double rate, int updatedAt, ScheduledEquityGrantRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public ScheduledEquityGrantRollupStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled equity grant rollup in status new.</summary>
    public static ScheduledEquityGrantRollup NewScheduledEquityGrantRollup(string code, string label)
    {
        return new ScheduledEquityGrantRollup(code, label, 0, 0.0, 0, ScheduledEquityGrantRollupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledEquityGrantRollupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledEquityGrantRollupStatus next)
    {
        switch (Status)
        {
            case ScheduledEquityGrantRollupStatus.New:
                return next == ScheduledEquityGrantRollupStatus.Running;
            case ScheduledEquityGrantRollupStatus.Queued:
                switch (next)
                {
                    case ScheduledEquityGrantRollupStatus.Running:
                    case ScheduledEquityGrantRollupStatus.Done:
                        return true;
                }
                return false;
            case ScheduledEquityGrantRollupStatus.Running:
                return next == ScheduledEquityGrantRollupStatus.Failed;
            case ScheduledEquityGrantRollupStatus.Done:
                return next == ScheduledEquityGrantRollupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScheduledEquityGrantRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled equity grant rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveValue()
    {
        return Math.Round(Rate + (double)Quantity, 2);
    }

    public ScheduledEquityGrantRollup Copy()
    {
        return new ScheduledEquityGrantRollup(Code, Label, Quantity, Rate, UpdatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
