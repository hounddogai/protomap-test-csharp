namespace Acme.Workforce.Absence.LeaveRequest.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped leave request baseline is in its life cycle.</summary>
public enum CappedLeaveRequestBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a capped leave request baseline and its current status.</summary>
public sealed class CappedLeaveRequestBaseline(string code, string title, double fte, int batchSize, Dictionary<string, string> attributes, int capacity, double cost, int retries, string description, string country, CappedLeaveRequestBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public int BatchSize { get; set; } = batchSize;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Capacity { get; set; } = capacity;
    public double Cost { get; set; } = cost;
    public int Retries { get; set; } = retries;
    public string Description { get; set; } = description;
    public string Country { get; set; } = country;
    public CappedLeaveRequestBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a capped leave request baseline in status planned.</summary>
    public static CappedLeaveRequestBaseline NewCappedLeaveRequestBaseline(string code, string title)
    {
        return new CappedLeaveRequestBaseline(
            code,
            title,
            0.0,
            0,
            new Dictionary<string, string>(),
            0,
            0.0,
            0,
            "",
            "",
            CappedLeaveRequestBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedLeaveRequestBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedLeaveRequestBaselineStatus next)
    {
        switch (Status)
        {
            case CappedLeaveRequestBaselineStatus.Planned:
                switch (next)
                {
                    case CappedLeaveRequestBaselineStatus.Scheduled:
                    case CappedLeaveRequestBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedLeaveRequestBaselineStatus.Scheduled:
                return next == CappedLeaveRequestBaselineStatus.InProgress || next == CappedLeaveRequestBaselineStatus.Completed;
            case CappedLeaveRequestBaselineStatus.InProgress:
                return next == CappedLeaveRequestBaselineStatus.Completed || next == CappedLeaveRequestBaselineStatus.Cancelled;
            case CappedLeaveRequestBaselineStatus.Completed:
                return next == CappedLeaveRequestBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CappedLeaveRequestBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped leave request baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped leave request baseline"} {Code}";
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

    public double ProjectedLoad()
    {
        return Math.Round((double)BatchSize - (double)Retries, 2);
    }

    public CappedLeaveRequestBaseline Copy()
    {
        return new CappedLeaveRequestBaseline(
            Code,
            Title,
            Fte,
            BatchSize,
            Attributes,
            Capacity,
            Cost,
            Retries,
            Description,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
