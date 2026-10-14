namespace Acme.Workforce.Talent.SkillProfile.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending skill profile group is in its life cycle.</summary>
public enum PendingSkillProfileGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a pending skill profile group and its current status.</summary>
public sealed class PendingSkillProfileGroup(string code, string title, double fte, string costCenter, double rate, int updatedAt, double discount, PendingSkillProfileGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string CostCenter { get; set; } = costCenter;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public double Discount { get; set; } = discount;
    public PendingSkillProfileGroupStatus Status { get; set; } = status;

    /// <summary>Starts a pending skill profile group in status planned.</summary>
    public static PendingSkillProfileGroup NewPendingSkillProfileGroup(string code, string title)
    {
        return new PendingSkillProfileGroup(
            code,
            title,
            0.0,
            "",
            0.0,
            0,
            0.0,
            PendingSkillProfileGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingSkillProfileGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingSkillProfileGroupStatus next)
    {
        switch (Status)
        {
            case PendingSkillProfileGroupStatus.Planned:
                switch (next)
                {
                    case PendingSkillProfileGroupStatus.Scheduled:
                    case PendingSkillProfileGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingSkillProfileGroupStatus.Scheduled:
                switch (next)
                {
                    case PendingSkillProfileGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingSkillProfileGroupStatus.InProgress:
                switch (next)
                {
                    case PendingSkillProfileGroupStatus.Completed:
                    case PendingSkillProfileGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingSkillProfileGroupStatus.Completed:
                return next == PendingSkillProfileGroupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PendingSkillProfileGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending skill profile group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"pending skill profile group"} {Code}";
    }

    public double ProjectedValue()
    {
        return Math.Round(Fte + Rate, 2);
    }

    public PendingSkillProfileGroup Copy()
    {
        return new PendingSkillProfileGroup(Code, Title, Fte, CostCenter, Rate, UpdatedAt, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
