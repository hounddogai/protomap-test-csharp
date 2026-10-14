namespace Acme.Workforce.Payrollops.RetroPay.Window;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay estimate is in its life cycle.</summary>
public enum RetroPayEstimateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A retro pay estimate as the retro pay team keeps it.</summary>
public sealed class RetroPayEstimate(string code, string title, string summary, string jobCode, bool active, bool approved, string description, double score, RetroPayEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string JobCode { get; set; } = jobCode;
    public bool Active { get; set; } = active;
    public bool Approved { get; set; } = approved;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public RetroPayEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay estimate in status planned.</summary>
    public static RetroPayEstimate NewRetroPayEstimate(string code, string title)
    {
        return new RetroPayEstimate(code, title, "", "", false, false, "", 0.0, RetroPayEstimateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayEstimateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayEstimateStatus next)
    {
        switch (Status)
        {
            case RetroPayEstimateStatus.Planned:
                switch (next)
                {
                    case RetroPayEstimateStatus.InProgress:
                    case RetroPayEstimateStatus.Completed:
                        return true;
                }
                return false;
            case RetroPayEstimateStatus.Scheduled:
                switch (next)
                {
                    case RetroPayEstimateStatus.InProgress:
                    case RetroPayEstimateStatus.Completed:
                        return true;
                }
                return false;
            case RetroPayEstimateStatus.InProgress:
                switch (next)
                {
                    case RetroPayEstimateStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayEstimateStatus.Completed:
                switch (next)
                {
                    case RetroPayEstimateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public RetroPayEstimate Copy()
    {
        return new RetroPayEstimate(Code, Title, Summary, JobCode, Active, Approved, Description, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
