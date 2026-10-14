namespace Acme.Workforce.Compliance.TrainingDue.DueVariance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered training due variance is in its life cycle.</summary>
public enum TieredTrainingDueVarianceStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered training due variance. Its code is unique within a tenant.</summary>
public sealed class TieredTrainingDueVariance(string code, string title, int quantity, string description, double fte, int durationMinutes, int tenureMonths, double budget, string segment, TieredTrainingDueVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Quantity { get; set; } = quantity;
    public string Description { get; set; } = description;
    public double Fte { get; set; } = fte;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int TenureMonths { get; set; } = tenureMonths;
    public double Budget { get; set; } = budget;
    public string Segment { get; set; } = segment;
    public TieredTrainingDueVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a tiered training due variance in status draft.</summary>
    public static TieredTrainingDueVariance NewTieredTrainingDueVariance(string code, string title)
    {
        return new TieredTrainingDueVariance(
            code,
            title,
            0,
            "",
            0.0,
            0,
            0,
            0.0,
            "",
            TieredTrainingDueVarianceStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredTrainingDueVarianceStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredTrainingDueVarianceStatus next)
    {
        switch (Status)
        {
            case TieredTrainingDueVarianceStatus.Draft:
                return next == TieredTrainingDueVarianceStatus.Pending || next == TieredTrainingDueVarianceStatus.Active;
            case TieredTrainingDueVarianceStatus.Pending:
                return next == TieredTrainingDueVarianceStatus.Active;
            case TieredTrainingDueVarianceStatus.Active:
                switch (next)
                {
                    case TieredTrainingDueVarianceStatus.Suspended:
                    case TieredTrainingDueVarianceStatus.Closed:
                        return true;
                }
                return false;
            case TieredTrainingDueVarianceStatus.Suspended:
                switch (next)
                {
                    case TieredTrainingDueVarianceStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(TieredTrainingDueVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered training due variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Fte + (double)DurationMinutes, 2);
    }

    public TieredTrainingDueVariance Copy()
    {
        return new TieredTrainingDueVariance(
            Code,
            Title,
            Quantity,
            Description,
            Fte,
            DurationMinutes,
            TenureMonths,
            Budget,
            Segment,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
