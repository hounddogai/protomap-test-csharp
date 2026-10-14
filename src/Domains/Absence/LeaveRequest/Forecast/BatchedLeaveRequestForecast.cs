namespace Acme.Workforce.Absence.LeaveRequest.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched leave request forecast is in its life cycle.</summary>
public enum BatchedLeaveRequestForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a batched leave request forecast and its current status.</summary>
public sealed class BatchedLeaveRequestForecast(string code, string name, double discount, int batchSize, string grade, int updatedAt, string title, Dictionary<string, string> attributes, string team, BatchedLeaveRequestForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public int BatchSize { get; set; } = batchSize;
    public string Grade { get; set; } = grade;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Team { get; set; } = team;
    public BatchedLeaveRequestForecastStatus Status { get; set; } = status;

    /// <summary>Starts a batched leave request forecast in status proposed.</summary>
    public static BatchedLeaveRequestForecast NewBatchedLeaveRequestForecast(string code, string name)
    {
        return new BatchedLeaveRequestForecast(
            code,
            name,
            0.0,
            0,
            "",
            0,
            "",
            new Dictionary<string, string>(),
            "",
            BatchedLeaveRequestForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedLeaveRequestForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedLeaveRequestForecastStatus next)
    {
        switch (Status)
        {
            case BatchedLeaveRequestForecastStatus.Proposed:
                switch (next)
                {
                    case BatchedLeaveRequestForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case BatchedLeaveRequestForecastStatus.Approved:
                return next == BatchedLeaveRequestForecastStatus.Rejected || next == BatchedLeaveRequestForecastStatus.Withdrawn;
            case BatchedLeaveRequestForecastStatus.Rejected:
                return next == BatchedLeaveRequestForecastStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(BatchedLeaveRequestForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched leave request forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedTotal()
    {
        return Math.Round((double)BatchSize - Discount, 2);
    }

    public BatchedLeaveRequestForecast Copy()
    {
        return new BatchedLeaveRequestForecast(
            Code,
            Name,
            Discount,
            BatchSize,
            Grade,
            UpdatedAt,
            Title,
            Attributes,
            Team,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
