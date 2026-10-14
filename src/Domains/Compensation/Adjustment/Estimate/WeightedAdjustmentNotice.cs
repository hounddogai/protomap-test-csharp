namespace Acme.Workforce.Compensation.Adjustment.Estimate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted adjustment notice is in its life cycle.</summary>
public enum WeightedAdjustmentNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One weighted adjustment notice, with what the planners need to act on it.</summary>
public sealed class WeightedAdjustmentNotice(string code, string title, string team, string contactEmail, string region, int tenureMonths, WeightedAdjustmentNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string ContactEmail { get; set; } = contactEmail;
    public string Region { get; set; } = region;
    public int TenureMonths { get; set; } = tenureMonths;
    public WeightedAdjustmentNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a weighted adjustment notice in status created.</summary>
    public static WeightedAdjustmentNotice NewWeightedAdjustmentNotice(string code, string title)
    {
        return new WeightedAdjustmentNotice(code, title, "", "", "", 0, WeightedAdjustmentNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedAdjustmentNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedAdjustmentNoticeStatus next)
    {
        switch (Status)
        {
            case WeightedAdjustmentNoticeStatus.Created:
                return next == WeightedAdjustmentNoticeStatus.Retired;
            case WeightedAdjustmentNoticeStatus.Validated:
                switch (next)
                {
                    case WeightedAdjustmentNoticeStatus.Published:
                        return true;
                }
                return false;
            case WeightedAdjustmentNoticeStatus.Published:
                return next == WeightedAdjustmentNoticeStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WeightedAdjustmentNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted adjustment notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted adjustment notice"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public WeightedAdjustmentNotice Copy()
    {
        return new WeightedAdjustmentNotice(Code, Title, Team, ContactEmail, Region, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
