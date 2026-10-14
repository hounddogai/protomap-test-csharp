namespace Acme.Workforce.Contractors.Rate.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rate tier is in its life cycle.</summary>
public enum RateTierStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a rate tier and its current status.</summary>
public sealed class RateTier(string code, string name, double ceiling, string jobCode, double margin, string badge, RateTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Ceiling { get; set; } = ceiling;
    public string JobCode { get; set; } = jobCode;
    public double Margin { get; set; } = margin;
    public string Badge { get; set; } = badge;
    public RateTierStatus Status { get; set; } = status;

    /// <summary>Starts a rate tier in status planned.</summary>
    public static RateTier NewRateTier(string code, string name)
    {
        return new RateTier(code, name, 0.0, "", 0.0, "", RateTierStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RateTierStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RateTierStatus next)
    {
        switch (Status)
        {
            case RateTierStatus.Planned:
                switch (next)
                {
                    case RateTierStatus.Scheduled:
                    case RateTierStatus.InProgress:
                        return true;
                }
                return false;
            case RateTierStatus.Scheduled:
                switch (next)
                {
                    case RateTierStatus.InProgress:
                        return true;
                }
                return false;
            case RateTierStatus.InProgress:
                return next == RateTierStatus.Completed || next == RateTierStatus.Cancelled;
            case RateTierStatus.Completed:
                switch (next)
                {
                    case RateTierStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RateTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rate tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetTotal()
    {
        return Math.Round(Margin * Ceiling, 2);
    }

    public RateTier Copy()
    {
        return new RateTier(Code, Name, Ceiling, JobCode, Margin, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
