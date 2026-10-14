namespace Acme.Workforce.Compensation.PayBand.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pay band request is in its life cycle.</summary>
public enum PayBandRequestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a pay band request and its current status.</summary>
public sealed class PayBandRequest(string code, string name, double margin, string contactEmail, int priority, double cost, PayBandRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Margin { get; set; } = margin;
    public string ContactEmail { get; set; } = contactEmail;
    public int Priority { get; set; } = priority;
    public double Cost { get; set; } = cost;
    public PayBandRequestStatus Status { get; set; } = status;

    /// <summary>Starts a pay band request in status planned.</summary>
    public static PayBandRequest NewPayBandRequest(string code, string name)
    {
        return new PayBandRequest(code, name, 0.0, "", 0, 0.0, PayBandRequestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PayBandRequestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PayBandRequestStatus next)
    {
        switch (Status)
        {
            case PayBandRequestStatus.Planned:
                switch (next)
                {
                    case PayBandRequestStatus.Completed:
                        return true;
                }
                return false;
            case PayBandRequestStatus.Scheduled:
                switch (next)
                {
                    case PayBandRequestStatus.InProgress:
                    case PayBandRequestStatus.Completed:
                        return true;
                }
                return false;
            case PayBandRequestStatus.InProgress:
                return next == PayBandRequestStatus.Completed || next == PayBandRequestStatus.Cancelled;
            case PayBandRequestStatus.Completed:
                return next == PayBandRequestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PayBandRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pay band request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Priority + Margin, 2);
    }

    public PayBandRequest Copy()
    {
        return new PayBandRequest(Code, Name, Margin, ContactEmail, Priority, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
