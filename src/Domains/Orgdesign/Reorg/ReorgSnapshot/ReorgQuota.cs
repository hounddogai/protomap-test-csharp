namespace Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg quota is in its life cycle.</summary>
public enum ReorgQuotaStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A reorg quota as the reorg team keeps it.</summary>
public sealed class ReorgQuota(string code, string name, double discount, double score, bool archived, string ownerId, string channel, string timezone, ReorgQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Discount { get; set; } = discount;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public string OwnerId { get; set; } = ownerId;
    public string Channel { get; set; } = channel;
    public string Timezone { get; set; } = timezone;
    public ReorgQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a reorg quota in status planned.</summary>
    public static ReorgQuota NewReorgQuota(string code, string name)
    {
        return new ReorgQuota(code, name, 0.0, 0.0, false, "", "", "", ReorgQuotaStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgQuotaStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgQuotaStatus next)
    {
        switch (Status)
        {
            case ReorgQuotaStatus.Planned:
                return next == ReorgQuotaStatus.Scheduled;
            case ReorgQuotaStatus.Scheduled:
                return next == ReorgQuotaStatus.Cancelled;
            case ReorgQuotaStatus.InProgress:
                switch (next)
                {
                    case ReorgQuotaStatus.Completed:
                    case ReorgQuotaStatus.Cancelled:
                        return true;
                }
                return false;
            case ReorgQuotaStatus.Completed:
                switch (next)
                {
                    case ReorgQuotaStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ReorgQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"reorg quota"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Score * Discount, 2);
    }

    public ReorgQuota Copy()
    {
        return new ReorgQuota(Code, Name, Discount, Score, Archived, OwnerId, Channel, Timezone, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
