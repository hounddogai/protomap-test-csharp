namespace Acme.Workforce.Orgdesign.Reorg.ReorgPlan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict reorg estimate is in its life cycle.</summary>
public enum StrictReorgEstimateStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a strict reorg estimate and its current status.</summary>
public sealed class StrictReorgEstimate(string code, string name, double floorValue, double score, int attempts, StrictReorgEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public double Score { get; set; } = score;
    public int Attempts { get; set; } = attempts;
    public StrictReorgEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a strict reorg estimate in status idle.</summary>
    public static StrictReorgEstimate NewStrictReorgEstimate(string code, string name)
    {
        return new StrictReorgEstimate(code, name, 0.0, 0.0, 0, StrictReorgEstimateStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictReorgEstimateStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictReorgEstimateStatus next)
    {
        switch (Status)
        {
            case StrictReorgEstimateStatus.Idle:
                switch (next)
                {
                    case StrictReorgEstimateStatus.Warming:
                    case StrictReorgEstimateStatus.Ready:
                        return true;
                }
                return false;
            case StrictReorgEstimateStatus.Warming:
                switch (next)
                {
                    case StrictReorgEstimateStatus.Ready:
                    case StrictReorgEstimateStatus.Stopped:
                        return true;
                }
                return false;
            case StrictReorgEstimateStatus.Ready:
                return next == StrictReorgEstimateStatus.Stopped;
            case StrictReorgEstimateStatus.Draining:
                return next == StrictReorgEstimateStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(StrictReorgEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict reorg estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedTotal()
    {
        return Math.Round(FloorValue * (double)Attempts, 2);
    }

    public StrictReorgEstimate Copy()
    {
        return new StrictReorgEstimate(Code, Name, FloorValue, Score, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
