namespace Acme.Workforce.Orgdesign.Headcount.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount quota is in its life cycle.</summary>
public enum HeadcountQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A headcount quota as the headcount team keeps it.</summary>
public sealed class HeadcountQuota(string code, string label, double cost, int expiresAt, string channel, string description, double score, bool approved, int startsAt, double maximum, int priority, HeadcountQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Cost { get; set; } = cost;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Channel { get; set; } = channel;
    public string Description { get; set; } = description;
    public double Score { get; set; } = score;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public double Maximum { get; set; } = maximum;
    public int Priority { get; set; } = priority;
    public HeadcountQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a headcount quota in status created.</summary>
    public static HeadcountQuota NewHeadcountQuota(string code, string label)
    {
        return new HeadcountQuota(code, label, 0.0, 0, "", "", 0.0, false, 0, 0.0, 0, HeadcountQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountQuotaStatus next)
    {
        switch (Status)
        {
            case HeadcountQuotaStatus.Created:
                return next == HeadcountQuotaStatus.Validated || next == HeadcountQuotaStatus.Published;
            case HeadcountQuotaStatus.Validated:
                switch (next)
                {
                    case HeadcountQuotaStatus.Published:
                        return true;
                }
                return false;
            case HeadcountQuotaStatus.Published:
                switch (next)
                {
                    case HeadcountQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Maximum + (double)StartsAt, 2);
    }

    public HeadcountQuota Copy()
    {
        return new HeadcountQuota(
            Code,
            Label,
            Cost,
            ExpiresAt,
            Channel,
            Description,
            Score,
            Approved,
            StartsAt,
            Maximum,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
