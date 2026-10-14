namespace Acme.Workforce.Compensation.MeritCycle.CycleOverride;

using Acme.Workforce.Kit.Text;

/// <summary>Where a merit cycle rule is in its life cycle.</summary>
public enum MeritCycleRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One merit cycle rule, with what the planners need to act on it.</summary>
public sealed class MeritCycleRule(string code, string title, string notes, bool approved, string name, string badge, double amount, MeritCycleRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Notes { get; set; } = notes;
    public bool Approved { get; set; } = approved;
    public string Name { get; set; } = name;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public MeritCycleRuleStatus Status { get; set; } = status;

    /// <summary>Starts a merit cycle rule in status planned.</summary>
    public static MeritCycleRule NewMeritCycleRule(string code, string title)
    {
        return new MeritCycleRule(code, title, "", false, "", "", 0.0, MeritCycleRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MeritCycleRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MeritCycleRuleStatus next)
    {
        switch (Status)
        {
            case MeritCycleRuleStatus.Planned:
                switch (next)
                {
                    case MeritCycleRuleStatus.InProgress:
                    case MeritCycleRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case MeritCycleRuleStatus.Scheduled:
                switch (next)
                {
                    case MeritCycleRuleStatus.InProgress:
                    case MeritCycleRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case MeritCycleRuleStatus.InProgress:
                switch (next)
                {
                    case MeritCycleRuleStatus.Completed:
                    case MeritCycleRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case MeritCycleRuleStatus.Completed:
                switch (next)
                {
                    case MeritCycleRuleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(MeritCycleRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"merit cycle rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public MeritCycleRule Copy()
    {
        return new MeritCycleRule(Code, Title, Notes, Approved, Name, Badge, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
