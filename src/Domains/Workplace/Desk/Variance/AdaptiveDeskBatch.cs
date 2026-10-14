namespace Acme.Workforce.Workplace.Desk.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive desk batch is in its life cycle.</summary>
public enum AdaptiveDeskBatchStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One adaptive desk batch, with what the planners need to act on it.</summary>
public sealed class AdaptiveDeskBatch(string code, string title, string segment, double discount, string salaryBand, double amount, int retries, AdaptiveDeskBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Segment { get; set; } = segment;
    public double Discount { get; set; } = discount;
    public string SalaryBand { get; set; } = salaryBand;
    public double Amount { get; set; } = amount;
    public int Retries { get; set; } = retries;
    public AdaptiveDeskBatchStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive desk batch in status open.</summary>
    public static AdaptiveDeskBatch NewAdaptiveDeskBatch(string code, string title)
    {
        return new AdaptiveDeskBatch(code, title, "", 0.0, "", 0.0, 0, AdaptiveDeskBatchStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveDeskBatchStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveDeskBatchStatus next)
    {
        switch (Status)
        {
            case AdaptiveDeskBatchStatus.Open:
                return next == AdaptiveDeskBatchStatus.Reopened;
            case AdaptiveDeskBatchStatus.InReview:
                return next == AdaptiveDeskBatchStatus.Resolved || next == AdaptiveDeskBatchStatus.Reopened;
            case AdaptiveDeskBatchStatus.Resolved:
                return next == AdaptiveDeskBatchStatus.Reopened || next == AdaptiveDeskBatchStatus.Archived;
            case AdaptiveDeskBatchStatus.Reopened:
                switch (next)
                {
                    case AdaptiveDeskBatchStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveDeskBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive desk batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"adaptive desk batch"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round(Amount * Discount, 2);
    }

    public AdaptiveDeskBatch Copy()
    {
        return new AdaptiveDeskBatch(Code, Title, Segment, Discount, SalaryBand, Amount, Retries, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
