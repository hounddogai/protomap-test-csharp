namespace Acme.Workforce.Talent.SkillProfile.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile baseline is in its life cycle.</summary>
public enum SkillProfileBaselineStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One skill profile baseline, with what the planners need to act on it.</summary>
public sealed class SkillProfileBaseline(string code, string title, double amount, string unit, string badge, string department, double weight, double rate, int endsAt, double discount, SkillProfileBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Amount { get; set; } = amount;
    public string Unit { get; set; } = unit;
    public string Badge { get; set; } = badge;
    public string Department { get; set; } = department;
    public double Weight { get; set; } = weight;
    public double Rate { get; set; } = rate;
    public int EndsAt { get; set; } = endsAt;
    public double Discount { get; set; } = discount;
    public SkillProfileBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile baseline in status planned.</summary>
    public static SkillProfileBaseline NewSkillProfileBaseline(string code, string title)
    {
        return new SkillProfileBaseline(
            code,
            title,
            0.0,
            "",
            "",
            "",
            0.0,
            0.0,
            0,
            0.0,
            SkillProfileBaselineStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileBaselineStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileBaselineStatus next)
    {
        switch (Status)
        {
            case SkillProfileBaselineStatus.Planned:
                switch (next)
                {
                    case SkillProfileBaselineStatus.InProgress:
                    case SkillProfileBaselineStatus.Cancelled:
                        return true;
                }
                return false;
            case SkillProfileBaselineStatus.Scheduled:
                return next == SkillProfileBaselineStatus.InProgress || next == SkillProfileBaselineStatus.Completed;
            case SkillProfileBaselineStatus.InProgress:
                return next == SkillProfileBaselineStatus.Cancelled;
            case SkillProfileBaselineStatus.Completed:
                return next == SkillProfileBaselineStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(SkillProfileBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"skill profile baseline"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Discount * Amount, 2);
    }

    public SkillProfileBaseline Copy()
    {
        return new SkillProfileBaseline(
            Code,
            Title,
            Amount,
            Unit,
            Badge,
            Department,
            Weight,
            Rate,
            EndsAt,
            Discount,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
