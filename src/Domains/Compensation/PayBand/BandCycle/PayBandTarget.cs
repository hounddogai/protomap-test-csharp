namespace Acme.Workforce.Compensation.PayBand.BandCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band target is in its life cycle.</summary>
public enum PayBandTargetStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a pay band target and its current status.</summary>
public sealed class PayBandTarget(string code, string name, int limit, string jobCode, double rate, PayBandTargetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public string JobCode { get; set; } = jobCode;
    public double Rate { get; set; } = rate;
    public PayBandTargetStatus Status { get; set; } = status;

    /// <summary>Starts a pay band target in status planned.</summary>
    public static PayBandTarget NewPayBandTarget(string code, string name)
    {
        return new PayBandTarget(code, name, 0, "", 0.0, PayBandTargetStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandTargetStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandTargetStatus next)
    {
        switch (Status)
        {
            case PayBandTargetStatus.Planned:
                return next == PayBandTargetStatus.Completed;
            case PayBandTargetStatus.Scheduled:
                switch (next)
                {
                    case PayBandTargetStatus.Completed:
                        return true;
                }
                return false;
            case PayBandTargetStatus.InProgress:
                switch (next)
                {
                    case PayBandTargetStatus.Cancelled:
                        return true;
                }
                return false;
            case PayBandTargetStatus.Completed:
                return next == PayBandTargetStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PayBandTargetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band target {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedValue()
    {
        return Math.Round(Rate - (double)Limit, 2);
    }

    public PayBandTarget Copy()
    {
        return new PayBandTarget(Code, Name, Limit, JobCode, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
