namespace Acme.Workforce.Compensation.BonusPool.Batch;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool request is in its life cycle.</summary>
public enum BonusPoolRequestStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a bonus pool request and its current status.</summary>
public sealed class BonusPoolRequest(string code, string title, int batchSize, string channel, bool visible, int startsAt, string salaryBand, double cost, string grade, string notes, int durationMinutes, BonusPoolRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int BatchSize { get; set; } = batchSize;
    public string Channel { get; set; } = channel;
    public bool Visible { get; set; } = visible;
    public int StartsAt { get; set; } = startsAt;
    public string SalaryBand { get; set; } = salaryBand;
    public double Cost { get; set; } = cost;
    public string Grade { get; set; } = grade;
    public string Notes { get; set; } = notes;
    public int DurationMinutes { get; set; } = durationMinutes;
    public BonusPoolRequestStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool request in status proposed.</summary>
    public static BonusPoolRequest NewBonusPoolRequest(string code, string title)
    {
        return new BonusPoolRequest(
            code,
            title,
            0,
            "",
            false,
            0,
            "",
            0.0,
            "",
            "",
            0,
            BonusPoolRequestStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolRequestStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolRequestStatus next)
    {
        switch (Status)
        {
            case BonusPoolRequestStatus.Proposed:
                return next == BonusPoolRequestStatus.Rejected || next == BonusPoolRequestStatus.Withdrawn;
            case BonusPoolRequestStatus.Approved:
                return next == BonusPoolRequestStatus.Rejected || next == BonusPoolRequestStatus.Withdrawn;
            case BonusPoolRequestStatus.Rejected:
                switch (next)
                {
                    case BonusPoolRequestStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)StartsAt * (double)DurationMinutes, 2);
    }

    public BonusPoolRequest Copy()
    {
        return new BonusPoolRequest(
            Code,
            Title,
            BatchSize,
            Channel,
            Visible,
            StartsAt,
            SalaryBand,
            Cost,
            Grade,
            Notes,
            DurationMinutes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
