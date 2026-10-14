namespace Acme.Workforce.Assets.Laptop.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched laptop variance is in its life cycle.</summary>
public enum BatchedLaptopVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One batched laptop variance, with what the planners need to act on it.</summary>
public sealed class BatchedLaptopVariance(string code, string title, double floorValue, double rate, string grade, string managerId, string badge, string ownerId, double amount, string currency, BatchedLaptopVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double FloorValue { get; set; } = floorValue;
    public double Rate { get; set; } = rate;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public string Badge { get; set; } = badge;
    public string OwnerId { get; set; } = ownerId;
    public double Amount { get; set; } = amount;
    public string Currency { get; set; } = currency;
    public BatchedLaptopVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a batched laptop variance in status proposed.</summary>
    public static BatchedLaptopVariance NewBatchedLaptopVariance(string code, string title)
    {
        return new BatchedLaptopVariance(
            code,
            title,
            0.0,
            0.0,
            "",
            "",
            "",
            "",
            0.0,
            "",
            BatchedLaptopVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedLaptopVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedLaptopVarianceStatus next)
    {
        switch (Status)
        {
            case BatchedLaptopVarianceStatus.Proposed:
                return next == BatchedLaptopVarianceStatus.Approved;
            case BatchedLaptopVarianceStatus.Approved:
                return next == BatchedLaptopVarianceStatus.Rejected || next == BatchedLaptopVarianceStatus.Withdrawn;
            case BatchedLaptopVarianceStatus.Rejected:
                return next == BatchedLaptopVarianceStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedLaptopVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched laptop variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetLoad()
    {
        return Math.Round(Amount + Rate, 2);
    }

    public BatchedLaptopVariance Copy()
    {
        return new BatchedLaptopVariance(
            Code,
            Title,
            FloorValue,
            Rate,
            Grade,
            ManagerId,
            Badge,
            OwnerId,
            Amount,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
