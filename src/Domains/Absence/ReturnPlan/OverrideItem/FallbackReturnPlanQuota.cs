namespace Acme.Workforce.Absence.ReturnPlan.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback return plan quota is in its life cycle.</summary>
public enum FallbackReturnPlanQuotaStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a fallback return plan quota and its current status.</summary>
public sealed class FallbackReturnPlanQuota(string code, string title, int expiresAt, int leaveDays, string segment, string location, int quantity, double rate, double score, string notes, FallbackReturnPlanQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int LeaveDays { get; set; } = leaveDays;
    public string Segment { get; set; } = segment;
    public string Location { get; set; } = location;
    public int Quantity { get; set; } = quantity;
    public double Rate { get; set; } = rate;
    public double Score { get; set; } = score;
    public string Notes { get; set; } = notes;
    public FallbackReturnPlanQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a fallback return plan quota in status draft.</summary>
    public static FallbackReturnPlanQuota NewFallbackReturnPlanQuota(string code, string title)
    {
        return new FallbackReturnPlanQuota(
            code,
            title,
            0,
            0,
            "",
            "",
            0,
            0.0,
            0.0,
            "",
            FallbackReturnPlanQuotaStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackReturnPlanQuotaStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackReturnPlanQuotaStatus next)
    {
        switch (Status)
        {
            case FallbackReturnPlanQuotaStatus.Draft:
                switch (next)
                {
                    case FallbackReturnPlanQuotaStatus.Pending:
                        return true;
                }
                return false;
            case FallbackReturnPlanQuotaStatus.Pending:
                return next == FallbackReturnPlanQuotaStatus.Active || next == FallbackReturnPlanQuotaStatus.Suspended;
            case FallbackReturnPlanQuotaStatus.Active:
                return next == FallbackReturnPlanQuotaStatus.Closed;
            case FallbackReturnPlanQuotaStatus.Suspended:
                switch (next)
                {
                    case FallbackReturnPlanQuotaStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackReturnPlanQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback return plan quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double WeightedVolume()
    {
        return Math.Round(Score - Rate, 2);
    }

    public FallbackReturnPlanQuota Copy()
    {
        return new FallbackReturnPlanQuota(
            Code,
            Title,
            ExpiresAt,
            LeaveDays,
            Segment,
            Location,
            Quantity,
            Rate,
            Score,
            Notes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
