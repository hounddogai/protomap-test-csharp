namespace Acme.Workforce.Contractors.Rate.RateBatch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted rate batch is in its life cycle.</summary>
public enum WeightedRateBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One weighted rate batch, with what the planners need to act on it.</summary>
public sealed class WeightedRateBatch(string code, string title, int capacity, double floorValue, int batchSize, string summary, double cost, WeightedRateBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public double FloorValue { get; set; } = floorValue;
    public int BatchSize { get; set; } = batchSize;
    public string Summary { get; set; } = summary;
    public double Cost { get; set; } = cost;
    public WeightedRateBatchStatus Status { get; set; } = status;

    /// <summary>Starts a weighted rate batch in status open.</summary>
    public static WeightedRateBatch NewWeightedRateBatch(string code, string title)
    {
        return new WeightedRateBatch(code, title, 0, 0.0, 0, "", 0.0, WeightedRateBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedRateBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedRateBatchStatus next)
    {
        switch (Status)
        {
            case WeightedRateBatchStatus.Open:
                return next == WeightedRateBatchStatus.InReview;
            case WeightedRateBatchStatus.InReview:
                switch (next)
                {
                    case WeightedRateBatchStatus.Archived:
                        return true;
                }
                return false;
            case WeightedRateBatchStatus.Resolved:
                return next == WeightedRateBatchStatus.Reopened || next == WeightedRateBatchStatus.Archived;
            case WeightedRateBatchStatus.Reopened:
                switch (next)
                {
                    case WeightedRateBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedRateBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted rate batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveValue()
    {
        return Math.Round(FloorValue + (double)Capacity, 2);
    }

    public WeightedRateBatch Copy()
    {
        return new WeightedRateBatch(Code, Title, Capacity, FloorValue, BatchSize, Summary, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
