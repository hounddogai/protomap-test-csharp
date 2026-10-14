namespace Acme.Workforce.Talent.SuccessionPlan.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary succession plan variance is in its life cycle.</summary>
public enum PrimarySuccessionPlanVarianceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a primary succession plan variance and its current status.</summary>
public sealed class PrimarySuccessionPlanVariance(string code, string title, double threshold, int capacity, int updatedAt, bool active, bool archived, double rate, int quantity, string unit, double margin, PrimarySuccessionPlanVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public int Capacity { get; set; } = capacity;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Active { get; set; } = active;
    public bool Archived { get; set; } = archived;
    public double Rate { get; set; } = rate;
    public int Quantity { get; set; } = quantity;
    public string Unit { get; set; } = unit;
    public double Margin { get; set; } = margin;
    public PrimarySuccessionPlanVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a primary succession plan variance in status planned.</summary>
    public static PrimarySuccessionPlanVariance NewPrimarySuccessionPlanVariance(string code, string title)
    {
        return new PrimarySuccessionPlanVariance(
            code,
            title,
            0.0,
            0,
            0,
            false,
            false,
            0.0,
            0,
            "",
            0.0,
            PrimarySuccessionPlanVarianceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySuccessionPlanVarianceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySuccessionPlanVarianceStatus next)
    {
        switch (Status)
        {
            case PrimarySuccessionPlanVarianceStatus.Planned:
                switch (next)
                {
                    case PrimarySuccessionPlanVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanVarianceStatus.Scheduled:
                switch (next)
                {
                    case PrimarySuccessionPlanVarianceStatus.Completed:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanVarianceStatus.InProgress:
                switch (next)
                {
                    case PrimarySuccessionPlanVarianceStatus.Completed:
                    case PrimarySuccessionPlanVarianceStatus.Cancelled:
                        return true;
                }
                return false;
            case PrimarySuccessionPlanVarianceStatus.Completed:
                return next == PrimarySuccessionPlanVarianceStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PrimarySuccessionPlanVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary succession plan variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"primary succession plan variance"} {Code}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)Quantity + Margin, 2);
    }

    public PrimarySuccessionPlanVariance Copy()
    {
        return new PrimarySuccessionPlanVariance(
            Code,
            Title,
            Threshold,
            Capacity,
            UpdatedAt,
            Active,
            Archived,
            Rate,
            Quantity,
            Unit,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
