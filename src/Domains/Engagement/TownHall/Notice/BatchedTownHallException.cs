namespace Acme.Workforce.Engagement.TownHall.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched town hall exception is in its life cycle.</summary>
public enum BatchedTownHallExceptionStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One batched town hall exception, with what the planners need to act on it.</summary>
public sealed class BatchedTownHallException(string code, string name, string country, double cost, string unit, double score, string locale, double rate, bool active, double rating, string title, BatchedTownHallExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Country { get; set; } = country;
    public double Cost { get; set; } = cost;
    public string Unit { get; set; } = unit;
    public double Score { get; set; } = score;
    public string Locale { get; set; } = locale;
    public double Rate { get; set; } = rate;
    public bool Active { get; set; } = active;
    public double Rating { get; set; } = rating;
    public string Title { get; set; } = title;
    public BatchedTownHallExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a batched town hall exception in status draft.</summary>
    public static BatchedTownHallException NewBatchedTownHallException(string code, string name)
    {
        return new BatchedTownHallException(
            code,
            name,
            "",
            0.0,
            "",
            0.0,
            "",
            0.0,
            false,
            0.0,
            "",
            BatchedTownHallExceptionStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedTownHallExceptionStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedTownHallExceptionStatus next)
    {
        switch (Status)
        {
            case BatchedTownHallExceptionStatus.Draft:
                switch (next)
                {
                    case BatchedTownHallExceptionStatus.Pending:
                    case BatchedTownHallExceptionStatus.Suspended:
                        return true;
                }
                return false;
            case BatchedTownHallExceptionStatus.Pending:
                return next == BatchedTownHallExceptionStatus.Active || next == BatchedTownHallExceptionStatus.Closed;
            case BatchedTownHallExceptionStatus.Active:
                switch (next)
                {
                    case BatchedTownHallExceptionStatus.Suspended:
                    case BatchedTownHallExceptionStatus.Closed:
                        return true;
                }
                return false;
            case BatchedTownHallExceptionStatus.Suspended:
                switch (next)
                {
                    case BatchedTownHallExceptionStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedTownHallExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched town hall exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Rate - Cost, 2);
    }

    public BatchedTownHallException Copy()
    {
        return new BatchedTownHallException(
            Code,
            Name,
            Country,
            Cost,
            Unit,
            Score,
            Locale,
            Rate,
            Active,
            Rating,
            Title,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
