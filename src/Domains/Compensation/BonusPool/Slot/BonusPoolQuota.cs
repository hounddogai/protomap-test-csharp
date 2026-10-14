namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool quota is in its life cycle.</summary>
public enum BonusPoolQuotaStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A bonus pool quota as the bonus pool team keeps it.</summary>
public sealed class BonusPoolQuota(string code, string name, string locale, List<string> tags, string contactEmail, int durationMinutes, string checksum, int expiresAt, string timezone, string phone, BonusPoolQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Locale { get; set; } = locale;
    public List<string> Tags { get; set; } = tags;
    public string ContactEmail { get; set; } = contactEmail;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Checksum { get; set; } = checksum;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Timezone { get; set; } = timezone;
    public string Phone { get; set; } = phone;
    public BonusPoolQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool quota in status open.</summary>
    public static BonusPoolQuota NewBonusPoolQuota(string code, string name)
    {
        return new BonusPoolQuota(
            code,
            name,
            "",
            new List<string>(),
            "",
            0,
            "",
            0,
            "",
            "",
            BonusPoolQuotaStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolQuotaStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolQuotaStatus next)
    {
        switch (Status)
        {
            case BonusPoolQuotaStatus.Open:
                return next == BonusPoolQuotaStatus.InReview || next == BonusPoolQuotaStatus.Reopened;
            case BonusPoolQuotaStatus.InReview:
                switch (next)
                {
                    case BonusPoolQuotaStatus.Resolved:
                    case BonusPoolQuotaStatus.Reopened:
                        return true;
                }
                return false;
            case BonusPoolQuotaStatus.Resolved:
                return next == BonusPoolQuotaStatus.Reopened;
            case BonusPoolQuotaStatus.Reopened:
                return next == BonusPoolQuotaStatus.Archived;
        }
        return false;
    }

    public void MoveTo(BonusPoolQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double NetLoad()
    {
        return Math.Round((double)ExpiresAt * (double)DurationMinutes, 2);
    }

    public BonusPoolQuota Copy()
    {
        return new BonusPoolQuota(
            Code,
            Name,
            Locale,
            Tags,
            ContactEmail,
            DurationMinutes,
            Checksum,
            ExpiresAt,
            Timezone,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
