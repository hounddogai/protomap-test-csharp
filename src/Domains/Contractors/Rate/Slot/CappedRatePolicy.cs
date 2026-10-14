namespace Acme.Workforce.Contractors.Rate.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped rate policy is in its life cycle.</summary>
public enum CappedRatePolicyStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One capped rate policy, with what the planners need to act on it.</summary>
public sealed class CappedRatePolicy(string code, string title, double ratio, int endsAt, int createdAt, double margin, string checksum, bool pinned, int version, int capacity, CappedRatePolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public int EndsAt { get; set; } = endsAt;
    public int CreatedAt { get; set; } = createdAt;
    public double Margin { get; set; } = margin;
    public string Checksum { get; set; } = checksum;
    public bool Pinned { get; set; } = pinned;
    public int Version { get; set; } = version;
    public int Capacity { get; set; } = capacity;
    public CappedRatePolicyStatus Status { get; set; } = status;

    /// <summary>Starts a capped rate policy in status planned.</summary>
    public static CappedRatePolicy NewCappedRatePolicy(string code, string title)
    {
        return new CappedRatePolicy(code, title, 0.0, 0, 0, 0.0, "", false, 0, 0, CappedRatePolicyStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedRatePolicyStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedRatePolicyStatus next)
    {
        switch (Status)
        {
            case CappedRatePolicyStatus.Planned:
                switch (next)
                {
                    case CappedRatePolicyStatus.Scheduled:
                        return true;
                }
                return false;
            case CappedRatePolicyStatus.Scheduled:
                switch (next)
                {
                    case CappedRatePolicyStatus.InProgress:
                        return true;
                }
                return false;
            case CappedRatePolicyStatus.InProgress:
                switch (next)
                {
                    case CappedRatePolicyStatus.Completed:
                    case CappedRatePolicyStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedRatePolicyStatus.Completed:
                switch (next)
                {
                    case CappedRatePolicyStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedRatePolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped rate policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Margin + (double)Version, 2);
    }

    public CappedRatePolicy Copy()
    {
        return new CappedRatePolicy(
            Code,
            Title,
            Ratio,
            EndsAt,
            CreatedAt,
            Margin,
            Checksum,
            Pinned,
            Version,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
