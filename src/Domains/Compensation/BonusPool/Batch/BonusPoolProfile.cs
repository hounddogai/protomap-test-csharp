namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool profile is in its life cycle.</summary>
public enum BonusPoolProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A bonus pool profile as the bonus pool team keeps it.</summary>
public sealed class BonusPoolProfile(string code, string name, double budget, string title, string salaryBand, BonusPoolProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Budget { get; set; } = budget;
    public string Title { get; set; } = title;
    public string SalaryBand { get; set; } = salaryBand;
    public BonusPoolProfileStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool profile in status planned.</summary>
    public static BonusPoolProfile NewBonusPoolProfile(string code, string name)
    {
        return new BonusPoolProfile(code, name, 0.0, "", "", BonusPoolProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolProfileStatus next)
    {
        switch (Status)
        {
            case BonusPoolProfileStatus.Planned:
                switch (next)
                {
                    case BonusPoolProfileStatus.Scheduled:
                    case BonusPoolProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolProfileStatus.Scheduled:
                switch (next)
                {
                    case BonusPoolProfileStatus.Completed:
                    case BonusPoolProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case BonusPoolProfileStatus.InProgress:
                return next == BonusPoolProfileStatus.Completed || next == BonusPoolProfileStatus.Cancelled;
            case BonusPoolProfileStatus.Completed:
                return next == BonusPoolProfileStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(BonusPoolProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Budget;
    }

    public BonusPoolProfile Copy()
    {
        return new BonusPoolProfile(Code, Name, Budget, Title, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
