namespace Acme.Workforce.Payrollops.RetroPay.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a retro pay policy is in its life cycle.</summary>
public enum RetroPayPolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One retro pay policy, with what the planners need to act on it.</summary>
public sealed class RetroPayPolicy(string code, string name, int priority, double discount, string department, int batchSize, string locale, string country, bool archived, RetroPayPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Priority { get; set; } = priority;
    public double Discount { get; set; } = discount;
    public string Department { get; set; } = department;
    public int BatchSize { get; set; } = batchSize;
    public string Locale { get; set; } = locale;
    public string Country { get; set; } = country;
    public bool Archived { get; set; } = archived;
    public RetroPayPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a retro pay policy in status planned.</summary>
    public static RetroPayPolicy NewRetroPayPolicy(string code, string name)
    {
        return new RetroPayPolicy(code, name, 0, 0.0, "", 0, "", "", false, RetroPayPolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RetroPayPolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RetroPayPolicyStatus next)
    {
        switch (Status)
        {
            case RetroPayPolicyStatus.Planned:
                switch (next)
                {
                    case RetroPayPolicyStatus.Completed:
                        return true;
                }
                return false;
            case RetroPayPolicyStatus.Scheduled:
                switch (next)
                {
                    case RetroPayPolicyStatus.InProgress:
                        return true;
                }
                return false;
            case RetroPayPolicyStatus.InProgress:
                switch (next)
                {
                    case RetroPayPolicyStatus.Completed:
                    case RetroPayPolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case RetroPayPolicyStatus.Completed:
                switch (next)
                {
                    case RetroPayPolicyStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RetroPayPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"retro pay policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(Discount - (double)BatchSize, 2);
    }

    public RetroPayPolicy Copy()
    {
        return new RetroPayPolicy(
            Code,
            Name,
            Priority,
            Discount,
            Department,
            BatchSize,
            Locale,
            Country,
            Archived,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
