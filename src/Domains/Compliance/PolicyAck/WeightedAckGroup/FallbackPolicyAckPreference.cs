namespace Acme.Workforce.Compliance.PolicyAck.WeightedAckGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback policy ack preference is in its life cycle.</summary>
public enum FallbackPolicyAckPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A fallback policy ack preference. Its code is unique within a tenant.</summary>
public sealed class FallbackPolicyAckPreference(string code, string title, double cost, string unit, double rate, int attempts, double discount, string country, FallbackPolicyAckPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public string Unit { get; set; } = unit;
    public double Rate { get; set; } = rate;
    public int Attempts { get; set; } = attempts;
    public double Discount { get; set; } = discount;
    public string Country { get; set; } = country;
    public FallbackPolicyAckPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a fallback policy ack preference in status planned.</summary>
    public static FallbackPolicyAckPreference NewFallbackPolicyAckPreference(string code, string title)
    {
        return new FallbackPolicyAckPreference(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0.0,
            "",
            FallbackPolicyAckPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackPolicyAckPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackPolicyAckPreferenceStatus next)
    {
        switch (Status)
        {
            case FallbackPolicyAckPreferenceStatus.Planned:
                switch (next)
                {
                    case FallbackPolicyAckPreferenceStatus.InProgress:
                    case FallbackPolicyAckPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackPolicyAckPreferenceStatus.Scheduled:
                switch (next)
                {
                    case FallbackPolicyAckPreferenceStatus.Completed:
                    case FallbackPolicyAckPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case FallbackPolicyAckPreferenceStatus.InProgress:
                return next == FallbackPolicyAckPreferenceStatus.Completed || next == FallbackPolicyAckPreferenceStatus.Cancelled;
            case FallbackPolicyAckPreferenceStatus.Completed:
                switch (next)
                {
                    case FallbackPolicyAckPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackPolicyAckPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback policy ack preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Discount * (double)Attempts, 2);
    }

    public FallbackPolicyAckPreference Copy()
    {
        return new FallbackPolicyAckPreference(Code, Title, Cost, Unit, Rate, Attempts, Discount, Country, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
