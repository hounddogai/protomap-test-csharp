namespace Acme.Workforce.Orgdesign.Headcount.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount estimate is in its life cycle.</summary>
public enum HeadcountEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a headcount estimate and its current status.</summary>
public sealed class HeadcountEstimate(string code, string label, bool approved, string location, int endsAt, HeadcountEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Approved { get; set; } = approved;
    public string Location { get; set; } = location;
    public int EndsAt { get; set; } = endsAt;
    public HeadcountEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a headcount estimate in status planned.</summary>
    public static HeadcountEstimate NewHeadcountEstimate(string code, string label)
    {
        return new HeadcountEstimate(code, label, false, "", 0, HeadcountEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountEstimateStatus next)
    {
        switch (Status)
        {
            case HeadcountEstimateStatus.Planned:
                return next == HeadcountEstimateStatus.InProgress || next == HeadcountEstimateStatus.Completed;
            case HeadcountEstimateStatus.Scheduled:
                switch (next)
                {
                    case HeadcountEstimateStatus.InProgress:
                    case HeadcountEstimateStatus.Completed:
                        return true;
                }
                return false;
            case HeadcountEstimateStatus.InProgress:
                return next == HeadcountEstimateStatus.Cancelled;
            case HeadcountEstimateStatus.Completed:
                switch (next)
                {
                    case HeadcountEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double MeasuredValue()
    {
        return (double)EndsAt;
    }

    public HeadcountEstimate Copy()
    {
        return new HeadcountEstimate(Code, Label, Approved, Location, EndsAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
