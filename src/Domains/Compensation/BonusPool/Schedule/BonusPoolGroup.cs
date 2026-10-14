namespace Acme.Workforce.Compensation.BonusPool.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool group is in its life cycle.</summary>
public enum BonusPoolGroupStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A bonus pool group as the bonus pool team keeps it.</summary>
public sealed class BonusPoolGroup(string code, string label, bool visible, double rate, string locale, string salaryBand, int leaveDays, double budget, string category, BonusPoolGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public bool Visible { get; set; } = visible;
    public double Rate { get; set; } = rate;
    public string Locale { get; set; } = locale;
    public string SalaryBand { get; set; } = salaryBand;
    public int LeaveDays { get; set; } = leaveDays;
    public double Budget { get; set; } = budget;
    public string Category { get; set; } = category;
    public BonusPoolGroupStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool group in status planned.</summary>
    public static BonusPoolGroup NewBonusPoolGroup(string code, string label)
    {
        return new BonusPoolGroup(code, label, false, 0.0, "", "", 0, 0.0, "", BonusPoolGroupStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolGroupStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolGroupStatus next)
    {
        switch (Status)
        {
            case BonusPoolGroupStatus.Planned:
                switch (next)
                {
                    case BonusPoolGroupStatus.InProgress:
                        return true;
                }
                return false;
            case BonusPoolGroupStatus.Scheduled:
                return next == BonusPoolGroupStatus.Completed || next == BonusPoolGroupStatus.Cancelled;
            case BonusPoolGroupStatus.InProgress:
                switch (next)
                {
                    case BonusPoolGroupStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolGroupStatus.Completed:
                return next == BonusPoolGroupStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BonusPoolGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetTotal()
    {
        return Math.Round(Rate + (double)LeaveDays, 2);
    }

    public BonusPoolGroup Copy()
    {
        return new BonusPoolGroup(
            Code,
            Label,
            Visible,
            Rate,
            Locale,
            SalaryBand,
            LeaveDays,
            Budget,
            Category,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
