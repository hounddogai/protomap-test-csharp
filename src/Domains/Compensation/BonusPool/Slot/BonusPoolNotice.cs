namespace Acme.Workforce.Compensation.BonusPool.Slot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool notice is in its life cycle.</summary>
public enum BonusPoolNoticeStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One bonus pool notice, with what the planners need to act on it.</summary>
public sealed class BonusPoolNotice(string code, string title, double threshold, string externalRef, double margin, double amount, int graceDays, double hours, double weight, string location, string ownerId, BonusPoolNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string ExternalRef { get; set; } = externalRef;
    public double Margin { get; set; } = margin;
    public double Amount { get; set; } = amount;
    public int GraceDays { get; set; } = graceDays;
    public double Hours { get; set; } = hours;
    public double Weight { get; set; } = weight;
    public string Location { get; set; } = location;
    public string OwnerId { get; set; } = ownerId;
    public BonusPoolNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool notice in status proposed.</summary>
    public static BonusPoolNotice NewBonusPoolNotice(string code, string title)
    {
        return new BonusPoolNotice(
            code,
            title,
            0.0,
            "",
            0.0,
            0.0,
            0,
            0.0,
            0.0,
            "",
            "",
            BonusPoolNoticeStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolNoticeStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolNoticeStatus next)
    {
        switch (Status)
        {
            case BonusPoolNoticeStatus.Proposed:
                return next == BonusPoolNoticeStatus.Rejected;
            case BonusPoolNoticeStatus.Approved:
                switch (next)
                {
                    case BonusPoolNoticeStatus.Rejected:
                    case BonusPoolNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
            case BonusPoolNoticeStatus.Rejected:
                switch (next)
                {
                    case BonusPoolNoticeStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double NetVolume()
    {
        return Math.Round(Margin + Threshold, 2);
    }

    public BonusPoolNotice Copy()
    {
        return new BonusPoolNotice(
            Code,
            Title,
            Threshold,
            ExternalRef,
            Margin,
            Amount,
            GraceDays,
            Hours,
            Weight,
            Location,
            OwnerId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
