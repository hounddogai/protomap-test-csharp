namespace Acme.Workforce.Orgdesign.Reorg.ReorgSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending reorg variance is in its life cycle.</summary>
public enum PendingReorgVarianceStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a pending reorg variance and its current status.</summary>
public sealed class PendingReorgVariance(string code, string name, int quantity, string description, double budget, bool approved, int durationMinutes, double threshold, PendingReorgVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public double Budget { get; set; } = budget;
    public bool Approved { get; set; } = approved;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double Threshold { get; set; } = threshold;
    public PendingReorgVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a pending reorg variance in status new.</summary>
    public static PendingReorgVariance NewPendingReorgVariance(string code, string name)
    {
        return new PendingReorgVariance(code, name, 0, "", 0.0, false, 0, 0.0, PendingReorgVarianceStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingReorgVarianceStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingReorgVarianceStatus next)
    {
        switch (Status)
        {
            case PendingReorgVarianceStatus.New:
                switch (next)
                {
                    case PendingReorgVarianceStatus.Done:
                        return true;
                }
                return false;
            case PendingReorgVarianceStatus.Queued:
                return next == PendingReorgVarianceStatus.Failed;
            case PendingReorgVarianceStatus.Running:
                return next == PendingReorgVarianceStatus.Done;
            case PendingReorgVarianceStatus.Done:
                switch (next)
                {
                    case PendingReorgVarianceStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingReorgVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending reorg variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending reorg variance"} {Code}";
    }

    public double WeightedTotal()
    {
        return Math.Round(Threshold * (double)Quantity, 2);
    }

    public PendingReorgVariance Copy()
    {
        return new PendingReorgVariance(
            Code,
            Name,
            Quantity,
            Description,
            Budget,
            Approved,
            DurationMinutes,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
