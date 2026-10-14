namespace Acme.Workforce.Compensation.BonusPool.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted bonus pool score is in its life cycle.</summary>
public enum WeightedBonusPoolScoreStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A weighted bonus pool score as the bonus pool team keeps it.</summary>
public sealed class WeightedBonusPoolScore(string code, string label, int updatedAt, List<string> tags, double threshold, int capacity, string timezone, string channel, WeightedBonusPoolScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int UpdatedAt { get; set; } = updatedAt;
    public List<string> Tags { get; set; } = tags;
    public double Threshold { get; set; } = threshold;
    public int Capacity { get; set; } = capacity;
    public string Timezone { get; set; } = timezone;
    public string Channel { get; set; } = channel;
    public WeightedBonusPoolScoreStatus Status { get; set; } = status;

    /// <summary>Starts a weighted bonus pool score in status created.</summary>
    public static WeightedBonusPoolScore NewWeightedBonusPoolScore(string code, string label)
    {
        return new WeightedBonusPoolScore(
            code,
            label,
            0,
            new List<string>(),
            0.0,
            0,
            "",
            "",
            WeightedBonusPoolScoreStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedBonusPoolScoreStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedBonusPoolScoreStatus next)
    {
        switch (Status)
        {
            case WeightedBonusPoolScoreStatus.Created:
                return next == WeightedBonusPoolScoreStatus.Validated || next == WeightedBonusPoolScoreStatus.Retired;
            case WeightedBonusPoolScoreStatus.Validated:
                switch (next)
                {
                    case WeightedBonusPoolScoreStatus.Retired:
                        return true;
                }
                return false;
            case WeightedBonusPoolScoreStatus.Published:
                return next == WeightedBonusPoolScoreStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedBonusPoolScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted bonus pool score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
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

    public double ProjectedTotal()
    {
        return Math.Round((double)UpdatedAt + (double)Capacity, 2);
    }

    public WeightedBonusPoolScore Copy()
    {
        return new WeightedBonusPoolScore(
            Code,
            Label,
            UpdatedAt,
            Tags,
            Threshold,
            Capacity,
            Timezone,
            Channel,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
