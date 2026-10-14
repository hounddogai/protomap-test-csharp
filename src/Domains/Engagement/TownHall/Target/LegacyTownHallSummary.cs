namespace Acme.Workforce.Engagement.TownHall.Target;

using Acme.Workforce.Kit.Text;

/// <summary>Where a legacy town hall summary is in its life cycle.</summary>
public enum LegacyTownHallSummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A legacy town hall summary as the town hall team keeps it.</summary>
public sealed class LegacyTownHallSummary(string code, string label, double rate, int expiresAt, string grade, int graceDays, string jobCode, string department, double amount, double threshold, LegacyTownHallSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Rate { get; set; } = rate;
    public int ExpiresAt { get; set; } = expiresAt;
    public string Grade { get; set; } = grade;
    public int GraceDays { get; set; } = graceDays;
    public string JobCode { get; set; } = jobCode;
    public string Department { get; set; } = department;
    public double Amount { get; set; } = amount;
    public double Threshold { get; set; } = threshold;
    public LegacyTownHallSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a legacy town hall summary in status open.</summary>
    public static LegacyTownHallSummary NewLegacyTownHallSummary(string code, string label)
    {
        return new LegacyTownHallSummary(
            code,
            label,
            0.0,
            0,
            "",
            0,
            "",
            "",
            0.0,
            0.0,
            LegacyTownHallSummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LegacyTownHallSummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LegacyTownHallSummaryStatus next)
    {
        switch (Status)
        {
            case LegacyTownHallSummaryStatus.Open:
                return next == LegacyTownHallSummaryStatus.InReview;
            case LegacyTownHallSummaryStatus.InReview:
                return next == LegacyTownHallSummaryStatus.Resolved || next == LegacyTownHallSummaryStatus.Archived;
            case LegacyTownHallSummaryStatus.Resolved:
                return next == LegacyTownHallSummaryStatus.Archived;
            case LegacyTownHallSummaryStatus.Reopened:
                switch (next)
                {
                    case LegacyTownHallSummaryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LegacyTownHallSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"legacy town hall summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"legacy town hall summary"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round(Amount - (double)GraceDays, 2);
    }

    public LegacyTownHallSummary Copy()
    {
        return new LegacyTownHallSummary(
            Code,
            Label,
            Rate,
            ExpiresAt,
            Grade,
            GraceDays,
            JobCode,
            Department,
            Amount,
            Threshold,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
