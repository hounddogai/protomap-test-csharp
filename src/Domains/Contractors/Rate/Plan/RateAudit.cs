namespace Acme.Workforce.Contractors.Rate.Plan;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate audit is in its life cycle.</summary>
public enum RateAuditStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a rate audit and its current status.</summary>
public sealed class RateAudit(string code, string name, string phone, double rate, double discount, bool approved, int sequence, string ownerId, RateAuditStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public double Rate { get; set; } = rate;
    public double Discount { get; set; } = discount;
    public bool Approved { get; set; } = approved;
    public int Sequence { get; set; } = sequence;
    public string OwnerId { get; set; } = ownerId;
    public RateAuditStatus Status { get; set; } = status;

    /// <summary>Starts a rate audit in status planned.</summary>
    public static RateAudit NewRateAudit(string code, string name)
    {
        return new RateAudit(code, name, "", 0.0, 0.0, false, 0, "", RateAuditStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateAuditStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateAuditStatus next)
    {
        switch (Status)
        {
            case RateAuditStatus.Planned:
                switch (next)
                {
                    case RateAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case RateAuditStatus.Scheduled:
                switch (next)
                {
                    case RateAuditStatus.Cancelled:
                        return true;
                }
                return false;
            case RateAuditStatus.InProgress:
                return next == RateAuditStatus.Completed || next == RateAuditStatus.Cancelled;
            case RateAuditStatus.Completed:
                switch (next)
                {
                    case RateAuditStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateAuditStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate audit {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"rate audit"} {Code}";
    }

    public double NetVolume()
    {
        return Math.Round(Rate + Discount, 2);
    }

    public RateAudit Copy()
    {
        return new RateAudit(Code, Name, Phone, Rate, Discount, Approved, Sequence, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
