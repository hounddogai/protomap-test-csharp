namespace Acme.Workforce.Talent.Nomination.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched nomination forecast is in its life cycle.</summary>
public enum BatchedNominationForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One batched nomination forecast, with what the planners need to act on it.</summary>
public sealed class BatchedNominationForecast(string code, string title, int capacity, string ownerId, string region, int limit, int attempts, bool pinned, string currency, int leaveDays, double fte, BatchedNominationForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string OwnerId { get; set; } = ownerId;
    public string Region { get; set; } = region;
    public int Limit { get; set; } = limit;
    public int Attempts { get; set; } = attempts;
    public bool Pinned { get; set; } = pinned;
    public string Currency { get; set; } = currency;
    public int LeaveDays { get; set; } = leaveDays;
    public double Fte { get; set; } = fte;
    public BatchedNominationForecastStatus Status { get; set; } = status;

    /// <summary>Starts a batched nomination forecast in status proposed.</summary>
    public static BatchedNominationForecast NewBatchedNominationForecast(string code, string title)
    {
        return new BatchedNominationForecast(
            code,
            title,
            0,
            "",
            "",
            0,
            0,
            false,
            "",
            0,
            0.0,
            BatchedNominationForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedNominationForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedNominationForecastStatus next)
    {
        switch (Status)
        {
            case BatchedNominationForecastStatus.Proposed:
                switch (next)
                {
                    case BatchedNominationForecastStatus.Approved:
                        return true;
                }
                return false;
            case BatchedNominationForecastStatus.Approved:
                switch (next)
                {
                    case BatchedNominationForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedNominationForecastStatus.Rejected:
                switch (next)
                {
                    case BatchedNominationForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedNominationForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched nomination forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"batched nomination forecast"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)Limit + Fte, 2);
    }

    public BatchedNominationForecast Copy()
    {
        return new BatchedNominationForecast(
            Code,
            Title,
            Capacity,
            OwnerId,
            Region,
            Limit,
            Attempts,
            Pinned,
            Currency,
            LeaveDays,
            Fte,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
