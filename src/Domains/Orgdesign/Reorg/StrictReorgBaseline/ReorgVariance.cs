namespace Acme.Workforce.Orgdesign.Reorg.StrictReorgBaseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg variance is in its life cycle.</summary>
public enum ReorgVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A reorg variance. Its code is unique within a tenant.</summary>
public sealed class ReorgVariance(string code, string title, double floorValue, double margin, int priority, ReorgVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Margin { get; set; } = margin;
    public int Priority { get; set; } = priority;
    public ReorgVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a reorg variance in status planned.</summary>
    public static ReorgVariance NewReorgVariance(string code, string title)
    {
        return new ReorgVariance(code, title, 0.0, 0.0, 0, ReorgVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgVarianceStatus next)
    {
        switch (Status)
        {
            case ReorgVarianceStatus.Planned:
                return next == ReorgVarianceStatus.Completed;
            case ReorgVarianceStatus.Scheduled:
                return next == ReorgVarianceStatus.InProgress || next == ReorgVarianceStatus.Completed;
            case ReorgVarianceStatus.InProgress:
                return next == ReorgVarianceStatus.Completed || next == ReorgVarianceStatus.Cancelled;
            case ReorgVarianceStatus.Completed:
                switch (next)
                {
                    case ReorgVarianceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Priority - Margin, 2);
    }

    public ReorgVariance Copy()
    {
        return new ReorgVariance(Code, Title, FloorValue, Margin, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
