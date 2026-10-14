namespace Acme.Workforce.Compensation.BonusPool.Claim;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted bonus pool quota is in its life cycle.</summary>
public enum WeightedBonusPoolQuotaStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a weighted bonus pool quota and its current status.</summary>
public sealed class WeightedBonusPoolQuota(string code, string name, string checksum, Dictionary<string, string> attributes, string description, string channel, int tenureMonths, string team, int graceDays, double rating, int limit, WeightedBonusPoolQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Checksum { get; set; } = checksum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Team { get; set; } = team;
    public int GraceDays { get; set; } = graceDays;
    public double Rating { get; set; } = rating;
    public int Limit { get; set; } = limit;
    public WeightedBonusPoolQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a weighted bonus pool quota in status proposed.</summary>
    public static WeightedBonusPoolQuota NewWeightedBonusPoolQuota(string code, string name)
    {
        return new WeightedBonusPoolQuota(
            code,
            name,
            "",
            new Dictionary<string, string>(),
            "",
            "",
            0,
            "",
            0,
            0.0,
            0,
            WeightedBonusPoolQuotaStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBonusPoolQuotaStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBonusPoolQuotaStatus next)
    {
        switch (Status)
        {
            case WeightedBonusPoolQuotaStatus.Proposed:
                switch (next)
                {
                    case WeightedBonusPoolQuotaStatus.Approved:
                        return true;
                }
                return false;
            case WeightedBonusPoolQuotaStatus.Approved:
                switch (next)
                {
                    case WeightedBonusPoolQuotaStatus.Rejected:
                    case WeightedBonusPoolQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
            case WeightedBonusPoolQuotaStatus.Rejected:
                switch (next)
                {
                    case WeightedBonusPoolQuotaStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedBonusPoolQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted bonus pool quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)TenureMonths * (double)GraceDays, 2);
    }

    public WeightedBonusPoolQuota Copy()
    {
        return new WeightedBonusPoolQuota(
            Code,
            Name,
            Checksum,
            Attributes,
            Description,
            Channel,
            TenureMonths,
            Team,
            GraceDays,
            Rating,
            Limit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
