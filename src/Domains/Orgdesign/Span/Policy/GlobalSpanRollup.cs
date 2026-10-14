namespace Acme.Workforce.Orgdesign.Span.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global span rollup is in its life cycle.</summary>
public enum GlobalSpanRollupStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A global span rollup as the span team keeps it.</summary>
public sealed class GlobalSpanRollup(string code, string label, int revision, int quantity, string reviewer, bool active, bool approved, string channel, GlobalSpanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Revision { get; set; } = revision;
    public int Quantity { get; set; } = quantity;
    public string Reviewer { get; set; } = reviewer;
    public bool Active { get; set; } = active;
    public bool Approved { get; set; } = approved;
    public string Channel { get; set; } = channel;
    public GlobalSpanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a global span rollup in status idle.</summary>
    public static GlobalSpanRollup NewGlobalSpanRollup(string code, string label)
    {
        return new GlobalSpanRollup(code, label, 0, 0, "", false, false, "", GlobalSpanRollupStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalSpanRollupStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalSpanRollupStatus next)
    {
        switch (Status)
        {
            case GlobalSpanRollupStatus.Idle:
                switch (next)
                {
                    case GlobalSpanRollupStatus.Warming:
                        return true;
                }
                return false;
            case GlobalSpanRollupStatus.Warming:
                return next == GlobalSpanRollupStatus.Draining || next == GlobalSpanRollupStatus.Stopped;
            case GlobalSpanRollupStatus.Ready:
                switch (next)
                {
                    case GlobalSpanRollupStatus.Draining:
                        return true;
                }
                return false;
            case GlobalSpanRollupStatus.Draining:
                return next == GlobalSpanRollupStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(GlobalSpanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global span rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Quantity * (double)Revision, 2);
    }

    public GlobalSpanRollup Copy()
    {
        return new GlobalSpanRollup(Code, Label, Revision, Quantity, Reviewer, Active, Approved, Channel, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
