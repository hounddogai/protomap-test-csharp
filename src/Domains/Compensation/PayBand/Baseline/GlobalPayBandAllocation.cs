namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global pay band allocation is in its life cycle.</summary>
public enum GlobalPayBandAllocationStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a global pay band allocation and its current status.</summary>
public sealed class GlobalPayBandAllocation(string code, string title, string channel, string phone, bool visible, double cost, List<string> tags, string category, string currency, GlobalPayBandAllocationStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Channel { get; set; } = channel;
    public string Phone { get; set; } = phone;
    public bool Visible { get; set; } = visible;
    public double Cost { get; set; } = cost;
    public List<string> Tags { get; set; } = tags;
    public string Category { get; set; } = category;
    public string Currency { get; set; } = currency;
    public GlobalPayBandAllocationStatus Status { get; set; } = status;

    /// <summary>Starts a global pay band allocation in status planned.</summary>
    public static GlobalPayBandAllocation NewGlobalPayBandAllocation(string code, string title)
    {
        return new GlobalPayBandAllocation(
            code,
            title,
            "",
            "",
            false,
            0.0,
            new List<string>(),
            "",
            "",
            GlobalPayBandAllocationStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalPayBandAllocationStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalPayBandAllocationStatus next)
    {
        switch (Status)
        {
            case GlobalPayBandAllocationStatus.Planned:
                return next == GlobalPayBandAllocationStatus.Scheduled || next == GlobalPayBandAllocationStatus.Cancelled;
            case GlobalPayBandAllocationStatus.Scheduled:
                return next == GlobalPayBandAllocationStatus.InProgress;
            case GlobalPayBandAllocationStatus.InProgress:
                switch (next)
                {
                    case GlobalPayBandAllocationStatus.Completed:
                        return true;
                }
                return false;
            case GlobalPayBandAllocationStatus.Completed:
                switch (next)
                {
                    case GlobalPayBandAllocationStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalPayBandAllocationStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global pay band allocation {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double MeasuredValue()
    {
        return Cost;
    }

    public GlobalPayBandAllocation Copy()
    {
        return new GlobalPayBandAllocation(
            Code,
            Title,
            Channel,
            Phone,
            Visible,
            Cost,
            Tags,
            Category,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
