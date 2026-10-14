namespace Acme.Workforce.Compensation.PayBand.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tiered pay band score is in its life cycle.</summary>
public enum TieredPayBandScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A tiered pay band score. Its code is unique within a tenant.</summary>
public sealed class TieredPayBandScore(string code, string title, int leaveDays, string channel, string costCenter, double discount, string unit, int updatedAt, string grade, string timezone, TieredPayBandScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int LeaveDays { get; set; } = leaveDays;
    public string Channel { get; set; } = channel;
    public string CostCenter { get; set; } = costCenter;
    public double Discount { get; set; } = discount;
    public string Unit { get; set; } = unit;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Grade { get; set; } = grade;
    public string Timezone { get; set; } = timezone;
    public TieredPayBandScoreStatus Status { get; set; } = status;

    /// <summary>Starts a tiered pay band score in status draft.</summary>
    public static TieredPayBandScore NewTieredPayBandScore(string code, string title)
    {
        return new TieredPayBandScore(code, title, 0, "", "", 0.0, "", 0, "", "", TieredPayBandScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TieredPayBandScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TieredPayBandScoreStatus next)
    {
        switch (Status)
        {
            case TieredPayBandScoreStatus.Draft:
                switch (next)
                {
                    case TieredPayBandScoreStatus.Closed:
                        return true;
                }
                return false;
            case TieredPayBandScoreStatus.Pending:
                return next == TieredPayBandScoreStatus.Suspended;
            case TieredPayBandScoreStatus.Active:
                switch (next)
                {
                    case TieredPayBandScoreStatus.Suspended:
                    case TieredPayBandScoreStatus.Closed:
                        return true;
                }
                return false;
            case TieredPayBandScoreStatus.Suspended:
                return next == TieredPayBandScoreStatus.Closed;
        }
        return false;
    }

    public void MoveTo(TieredPayBandScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tiered pay band score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Discount * (double)LeaveDays, 2);
    }

    public TieredPayBandScore Copy()
    {
        return new TieredPayBandScore(
            Code,
            Title,
            LeaveDays,
            Channel,
            CostCenter,
            Discount,
            Unit,
            UpdatedAt,
            Grade,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
