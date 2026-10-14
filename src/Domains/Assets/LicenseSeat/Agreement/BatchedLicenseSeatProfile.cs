namespace Acme.Workforce.Assets.LicenseSeat.Agreement;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched license seat profile is in its life cycle.</summary>
public enum BatchedLicenseSeatProfileStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One batched license seat profile, with what the planners need to act on it.</summary>
public sealed class BatchedLicenseSeatProfile(string code, string name, int capacity, int createdAt, double budget, string jobCode, string badge, double floorValue, BatchedLicenseSeatProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int CreatedAt { get; set; } = createdAt;
    public double Budget { get; set; } = budget;
    public string JobCode { get; set; } = jobCode;
    public string Badge { get; set; } = badge;
    public double FloorValue { get; set; } = floorValue;
    public BatchedLicenseSeatProfileStatus Status { get; set; } = status;

    /// <summary>Starts a batched license seat profile in status draft.</summary>
    public static BatchedLicenseSeatProfile NewBatchedLicenseSeatProfile(string code, string name)
    {
        return new BatchedLicenseSeatProfile(
            code,
            name,
            0,
            0,
            0.0,
            "",
            "",
            0.0,
            BatchedLicenseSeatProfileStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedLicenseSeatProfileStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedLicenseSeatProfileStatus next)
    {
        switch (Status)
        {
            case BatchedLicenseSeatProfileStatus.Draft:
                switch (next)
                {
                    case BatchedLicenseSeatProfileStatus.Active:
                    case BatchedLicenseSeatProfileStatus.Closed:
                        return true;
                }
                return false;
            case BatchedLicenseSeatProfileStatus.Pending:
                return next == BatchedLicenseSeatProfileStatus.Closed;
            case BatchedLicenseSeatProfileStatus.Active:
                switch (next)
                {
                    case BatchedLicenseSeatProfileStatus.Closed:
                        return true;
                }
                return false;
            case BatchedLicenseSeatProfileStatus.Suspended:
                return next == BatchedLicenseSeatProfileStatus.Closed;
        }
        return false;
    }

    public void MoveTo(BatchedLicenseSeatProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched license seat profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedLoad()
    {
        return Math.Round(Budget * (double)Capacity, 2);
    }

    public BatchedLicenseSeatProfile Copy()
    {
        return new BatchedLicenseSeatProfile(
            Code,
            Name,
            Capacity,
            CreatedAt,
            Budget,
            JobCode,
            Badge,
            FloorValue,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
