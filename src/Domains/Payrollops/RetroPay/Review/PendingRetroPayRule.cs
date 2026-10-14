namespace Acme.Workforce.Payrollops.RetroPay.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending retro pay rule is in its life cycle.</summary>
public enum PendingRetroPayRuleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One pending retro pay rule, with what the planners need to act on it.</summary>
public sealed class PendingRetroPayRule(string code, string name, int capacity, int attempts, int leaveDays, string checksum, int tenureMonths, string notes, string reviewer, PendingRetroPayRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public int Attempts { get; set; } = attempts;
    public int LeaveDays { get; set; } = leaveDays;
    public string Checksum { get; set; } = checksum;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Notes { get; set; } = notes;
    public string Reviewer { get; set; } = reviewer;
    public PendingRetroPayRuleStatus Status { get; set; } = status;

    /// <summary>Starts a pending retro pay rule in status planned.</summary>
    public static PendingRetroPayRule NewPendingRetroPayRule(string code, string name)
    {
        return new PendingRetroPayRule(code, name, 0, 0, 0, "", 0, "", "", PendingRetroPayRuleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingRetroPayRuleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingRetroPayRuleStatus next)
    {
        switch (Status)
        {
            case PendingRetroPayRuleStatus.Planned:
                return next == PendingRetroPayRuleStatus.InProgress || next == PendingRetroPayRuleStatus.Completed;
            case PendingRetroPayRuleStatus.Scheduled:
                switch (next)
                {
                    case PendingRetroPayRuleStatus.InProgress:
                    case PendingRetroPayRuleStatus.Completed:
                        return true;
                }
                return false;
            case PendingRetroPayRuleStatus.InProgress:
                switch (next)
                {
                    case PendingRetroPayRuleStatus.Completed:
                    case PendingRetroPayRuleStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingRetroPayRuleStatus.Completed:
                switch (next)
                {
                    case PendingRetroPayRuleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingRetroPayRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending retro pay rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)LeaveDays - (double)TenureMonths, 2);
    }

    public PendingRetroPayRule Copy()
    {
        return new PendingRetroPayRule(
            Code,
            Name,
            Capacity,
            Attempts,
            LeaveDays,
            Checksum,
            TenureMonths,
            Notes,
            Reviewer,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
