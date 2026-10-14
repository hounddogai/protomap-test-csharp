namespace Acme.Workforce.Contractors.Rate.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate allocation is in its life cycle.</summary>
public enum RateAllocationStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a rate allocation and its current status.</summary>
public sealed class RateAllocation(string code, string label, string channel, int leaveDays, string source, int revision, RateAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int LeaveDays { get; set; } = leaveDays;
    public string Source { get; set; } = source;
    public int Revision { get; set; } = revision;
    public RateAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a rate allocation in status idle.</summary>
    public static RateAllocation NewRateAllocation(string code, string label)
    {
        return new RateAllocation(code, label, "", 0, "", 0, RateAllocationStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateAllocationStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateAllocationStatus next)
    {
        switch (Status)
        {
            case RateAllocationStatus.Idle:
                switch (next)
                {
                    case RateAllocationStatus.Warming:
                    case RateAllocationStatus.Stopped:
                        return true;
                }
                return false;
            case RateAllocationStatus.Warming:
                switch (next)
                {
                    case RateAllocationStatus.Draining:
                        return true;
                }
                return false;
            case RateAllocationStatus.Ready:
                return next == RateAllocationStatus.Stopped;
            case RateAllocationStatus.Draining:
                switch (next)
                {
                    case RateAllocationStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)LeaveDays + (double)Revision, 2);
    }

    public RateAllocation Copy()
    {
        return new RateAllocation(Code, Label, Channel, LeaveDays, Source, Revision, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
