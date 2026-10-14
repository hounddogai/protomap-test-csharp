namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool baseline is in its life cycle.</summary>
public enum BonusPoolBaselineStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A bonus pool baseline as the bonus pool team keeps it.</summary>
public sealed class BonusPoolBaseline(string code, string title, double cost, int leaveDays, string managerId, string channel, double maximum, double ceiling, double weight, double discount, double rate, BonusPoolBaselineStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Cost { get; set; } = cost;
    public int LeaveDays { get; set; } = leaveDays;
    public string ManagerId { get; set; } = managerId;
    public string Channel { get; set; } = channel;
    public double Maximum { get; set; } = maximum;
    public double Ceiling { get; set; } = ceiling;
    public double Weight { get; set; } = weight;
    public double Discount { get; set; } = discount;
    public double Rate { get; set; } = rate;
    public BonusPoolBaselineStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool baseline in status proposed.</summary>
    public static BonusPoolBaseline NewBonusPoolBaseline(string code, string title)
    {
        return new BonusPoolBaseline(
            code,
            title,
            0.0,
            0,
            "",
            "",
            0.0,
            0.0,
            0.0,
            0.0,
            0.0,
            BonusPoolBaselineStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolBaselineStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolBaselineStatus next)
    {
        switch (Status)
        {
            case BonusPoolBaselineStatus.Proposed:
                return next == BonusPoolBaselineStatus.Approved || next == BonusPoolBaselineStatus.Withdrawn;
            case BonusPoolBaselineStatus.Approved:
                switch (next)
                {
                    case BonusPoolBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
            case BonusPoolBaselineStatus.Rejected:
                switch (next)
                {
                    case BonusPoolBaselineStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolBaselineStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool baseline {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Discount * Cost, 2);
    }

    public BonusPoolBaseline Copy()
    {
        return new BonusPoolBaseline(
            Code,
            Title,
            Cost,
            LeaveDays,
            ManagerId,
            Channel,
            Maximum,
            Ceiling,
            Weight,
            Discount,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
