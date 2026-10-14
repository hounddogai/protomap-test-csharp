namespace Acme.Workforce.Payrollops.RetroPay.PayAssignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy retro pay summary is in its life cycle.</summary>
public enum LegacyRetroPaySummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One legacy retro pay summary, with what the planners need to act on it.</summary>
public sealed class LegacyRetroPaySummary(string code, string title, string externalRef, int leaveDays, int createdAt, double maximum, bool approved, int quantity, string region, LegacyRetroPaySummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public int LeaveDays { get; set; } = leaveDays;
    public int CreatedAt { get; set; } = createdAt;
    public double Maximum { get; set; } = maximum;
    public bool Approved { get; set; } = approved;
    public int Quantity { get; set; } = quantity;
    public string Region { get; set; } = region;
    public LegacyRetroPaySummaryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy retro pay summary in status proposed.</summary>
    public static LegacyRetroPaySummary NewLegacyRetroPaySummary(string code, string title)
    {
        return new LegacyRetroPaySummary(
            code,
            title,
            "",
            0,
            0,
            0.0,
            false,
            0,
            "",
            LegacyRetroPaySummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyRetroPaySummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyRetroPaySummaryStatus next)
    {
        switch (Status)
        {
            case LegacyRetroPaySummaryStatus.Proposed:
                return next == LegacyRetroPaySummaryStatus.Rejected || next == LegacyRetroPaySummaryStatus.Withdrawn;
            case LegacyRetroPaySummaryStatus.Approved:
                return next == LegacyRetroPaySummaryStatus.Rejected || next == LegacyRetroPaySummaryStatus.Withdrawn;
            case LegacyRetroPaySummaryStatus.Rejected:
                return next == LegacyRetroPaySummaryStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(LegacyRetroPaySummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy retro pay summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Maximum - (double)CreatedAt, 2);
    }

    public LegacyRetroPaySummary Copy()
    {
        return new LegacyRetroPaySummary(
            Code,
            Title,
            ExternalRef,
            LeaveDays,
            CreatedAt,
            Maximum,
            Approved,
            Quantity,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
