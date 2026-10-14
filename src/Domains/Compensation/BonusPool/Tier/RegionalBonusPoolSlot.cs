namespace Acme.Workforce.Compensation.BonusPool.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional bonus pool slot is in its life cycle.</summary>
public enum RegionalBonusPoolSlotStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One regional bonus pool slot, with what the planners need to act on it.</summary>
public sealed class RegionalBonusPoolSlot(string code, string label, string skill, int expiresAt, double minimum, int revision, string locale, string title, int updatedAt, int limit, string ownerId, RegionalBonusPoolSlotStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Minimum { get; set; } = minimum;
    public int Revision { get; set; } = revision;
    public string Locale { get; set; } = locale;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Limit { get; set; } = limit;
    public string OwnerId { get; set; } = ownerId;
    public RegionalBonusPoolSlotStatus Status { get; set; } = status;

    /// <summary>Starts a regional bonus pool slot in status new.</summary>
    public static RegionalBonusPoolSlot NewRegionalBonusPoolSlot(string code, string label)
    {
        return new RegionalBonusPoolSlot(
            code,
            label,
            "",
            0,
            0.0,
            0,
            "",
            "",
            0,
            0,
            "",
            RegionalBonusPoolSlotStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalBonusPoolSlotStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalBonusPoolSlotStatus next)
    {
        switch (Status)
        {
            case RegionalBonusPoolSlotStatus.New:
                switch (next)
                {
                    case RegionalBonusPoolSlotStatus.Running:
                        return true;
                }
                return false;
            case RegionalBonusPoolSlotStatus.Queued:
                return next == RegionalBonusPoolSlotStatus.Running;
            case RegionalBonusPoolSlotStatus.Running:
                return next == RegionalBonusPoolSlotStatus.Done || next == RegionalBonusPoolSlotStatus.Failed;
            case RegionalBonusPoolSlotStatus.Done:
                switch (next)
                {
                    case RegionalBonusPoolSlotStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalBonusPoolSlotStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional bonus pool slot {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string DisplayLabel()
    {
        return $"{"regional bonus pool slot"} {Code}";
    }

    public double NetTotal()
    {
        return Math.Round((double)Limit + Minimum, 2);
    }

    public RegionalBonusPoolSlot Copy()
    {
        return new RegionalBonusPoolSlot(
            Code,
            Label,
            Skill,
            ExpiresAt,
            Minimum,
            Revision,
            Locale,
            Title,
            UpdatedAt,
            Limit,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
