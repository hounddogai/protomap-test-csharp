namespace Acme.Workforce.Assets.LicenseSeat.SeatTemplate;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal license seat summary is in its life cycle.</summary>
public enum SeasonalLicenseSeatSummaryStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>The record of a seasonal license seat summary and its current status.</summary>
public sealed class SeasonalLicenseSeatSummary(string code, string title, int expiresAt, double hours, string skill, string unit, string summary, string reviewer, string department, string employeeId, string grade, SeasonalLicenseSeatSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public string Skill { get; set; } = skill;
    public string Unit { get; set; } = unit;
    public string Summary { get; set; } = summary;
    public string Reviewer { get; set; } = reviewer;
    public string Department { get; set; } = department;
    public string EmployeeId { get; set; } = employeeId;
    public string Grade { get; set; } = grade;
    public SeasonalLicenseSeatSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal license seat summary in status idle.</summary>
    public static SeasonalLicenseSeatSummary NewSeasonalLicenseSeatSummary(string code, string title)
    {
        return new SeasonalLicenseSeatSummary(
            code,
            title,
            0,
            0.0,
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            SeasonalLicenseSeatSummaryStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalLicenseSeatSummaryStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalLicenseSeatSummaryStatus next)
    {
        switch (Status)
        {
            case SeasonalLicenseSeatSummaryStatus.Idle:
                switch (next)
                {
                    case SeasonalLicenseSeatSummaryStatus.Warming:
                    case SeasonalLicenseSeatSummaryStatus.Ready:
                        return true;
                }
                return false;
            case SeasonalLicenseSeatSummaryStatus.Warming:
                return next == SeasonalLicenseSeatSummaryStatus.Draining || next == SeasonalLicenseSeatSummaryStatus.Stopped;
            case SeasonalLicenseSeatSummaryStatus.Ready:
                return next == SeasonalLicenseSeatSummaryStatus.Draining;
            case SeasonalLicenseSeatSummaryStatus.Draining:
                switch (next)
                {
                    case SeasonalLicenseSeatSummaryStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalLicenseSeatSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal license seat summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal license seat summary"} {Code}";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Hours - (double)ExpiresAt, 2);
    }

    public SeasonalLicenseSeatSummary Copy()
    {
        return new SeasonalLicenseSeatSummary(
            Code,
            Title,
            ExpiresAt,
            Hours,
            Skill,
            Unit,
            Summary,
            Reviewer,
            Department,
            EmployeeId,
            Grade,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
