namespace Acme.Workforce.Compensation.EquityGrant.GrantDigest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant threshold is in its life cycle.</summary>
public enum EquityGrantThresholdStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A equity grant threshold as the equity grant team keeps it.</summary>
public sealed class EquityGrantThreshold(string code, string name, double budget, string checksum, int attempts, string jobCode, string contactEmail, bool approved, EquityGrantThresholdStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Checksum { get; set; } = checksum;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public string ContactEmail { get; set; } = contactEmail;
    public bool Approved { get; set; } = approved;
    public EquityGrantThresholdStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant threshold in status planned.</summary>
    public static EquityGrantThreshold NewEquityGrantThreshold(string code, string name)
    {
        return new EquityGrantThreshold(code, name, 0.0, "", 0, "", "", false, EquityGrantThresholdStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantThresholdStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantThresholdStatus next)
    {
        switch (Status)
        {
            case EquityGrantThresholdStatus.Planned:
                return next == EquityGrantThresholdStatus.Scheduled || next == EquityGrantThresholdStatus.Cancelled;
            case EquityGrantThresholdStatus.Scheduled:
                return next == EquityGrantThresholdStatus.InProgress || next == EquityGrantThresholdStatus.Completed;
            case EquityGrantThresholdStatus.InProgress:
                switch (next)
                {
                    case EquityGrantThresholdStatus.Completed:
                        return true;
                }
                return false;
            case EquityGrantThresholdStatus.Completed:
                switch (next)
                {
                    case EquityGrantThresholdStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(EquityGrantThresholdStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant threshold {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Attempts + Budget, 2);
    }

    public EquityGrantThreshold Copy()
    {
        return new EquityGrantThreshold(
            Code,
            Name,
            Budget,
            Checksum,
            Attempts,
            JobCode,
            ContactEmail,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
