namespace Acme.Workforce.Payrollops.Garnishment.Reservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment quota is in its life cycle.</summary>
public enum GarnishmentQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A garnishment quota as the garnishment team keeps it.</summary>
public sealed class GarnishmentQuota(string code, string name, int revision, string source, int attempts, double minimum, double discount, int endsAt, int sequence, List<string> tags, string timezone, GarnishmentQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string Source { get; set; } = source;
    public int Attempts { get; set; } = attempts;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public int EndsAt { get; set; } = endsAt;
    public int Sequence { get; set; } = sequence;
    public List<string> Tags { get; set; } = tags;
    public string Timezone { get; set; } = timezone;
    public GarnishmentQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment quota in status proposed.</summary>
    public static GarnishmentQuota NewGarnishmentQuota(string code, string name)
    {
        return new GarnishmentQuota(
            code,
            name,
            0,
            "",
            0,
            0.0,
            0.0,
            0,
            0,
            new List<string>(),
            "",
            GarnishmentQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentQuotaStatus next)
    {
        switch (Status)
        {
            case GarnishmentQuotaStatus.Proposed:
                switch (next)
                {
                    case GarnishmentQuotaStatus.Approved:
                    case GarnishmentQuotaStatus.Rejected:
                        return true;
                }
                return false;
            case GarnishmentQuotaStatus.Approved:
                switch (next)
                {
                    case GarnishmentQuotaStatus.Rejected:
                    case GarnishmentQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case GarnishmentQuotaStatus.Rejected:
                return next == GarnishmentQuotaStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(GarnishmentQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double AdjustedTotal()
    {
        return Math.Round((double)Revision * (double)Sequence, 2);
    }

    public GarnishmentQuota Copy()
    {
        return new GarnishmentQuota(
            Code,
            Name,
            Revision,
            Source,
            Attempts,
            Minimum,
            Discount,
            EndsAt,
            Sequence,
            Tags,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
