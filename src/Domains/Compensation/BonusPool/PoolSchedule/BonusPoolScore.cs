namespace Acme.Workforce.Compensation.BonusPool.PoolSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool score is in its life cycle.</summary>
public enum BonusPoolScoreStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A bonus pool score. Its code is unique within a tenant.</summary>
public sealed class BonusPoolScore(string code, string title, int sequence, int updatedAt, Dictionary<string, string> attributes, BonusPoolScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Sequence { get; set; } = sequence;
    public int UpdatedAt { get; set; } = updatedAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public BonusPoolScoreStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool score in status planned.</summary>
    public static BonusPoolScore NewBonusPoolScore(string code, string title)
    {
        return new BonusPoolScore(code, title, 0, 0, new Dictionary<string, string>(), BonusPoolScoreStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolScoreStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolScoreStatus next)
    {
        switch (Status)
        {
            case BonusPoolScoreStatus.Planned:
                switch (next)
                {
                    case BonusPoolScoreStatus.Scheduled:
                    case BonusPoolScoreStatus.InProgress:
                        return true;
                }
                return false;
            case BonusPoolScoreStatus.Scheduled:
                return next == BonusPoolScoreStatus.Cancelled;
            case BonusPoolScoreStatus.InProgress:
                return next == BonusPoolScoreStatus.Cancelled;
            case BonusPoolScoreStatus.Completed:
                switch (next)
                {
                    case BonusPoolScoreStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double WeightedValue()
    {
        return Math.Round((double)UpdatedAt * (double)Sequence, 2);
    }

    public BonusPoolScore Copy()
    {
        return new BonusPoolScore(Code, Title, Sequence, UpdatedAt, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
