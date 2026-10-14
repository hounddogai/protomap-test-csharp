namespace Acme.Workforce.Assets.LicenseSeat.SeatForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a license seat estimate is in its life cycle.</summary>
public enum LicenseSeatEstimateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a license seat estimate and its current status.</summary>
public sealed class LicenseSeatEstimate(string code, string title, string region, string checksum, string label, string jobCode, int durationMinutes, double floorValue, int endsAt, LicenseSeatEstimateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Region { get; set; } = region;
    public string Checksum { get; set; } = checksum;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double FloorValue { get; set; } = floorValue;
    public int EndsAt { get; set; } = endsAt;
    public LicenseSeatEstimateStatus Status { get; set; } = status;

    /// <summary>Starts a license seat estimate in status open.</summary>
    public static LicenseSeatEstimate NewLicenseSeatEstimate(string code, string title)
    {
        return new LicenseSeatEstimate(code, title, "", "", "", "", 0, 0.0, 0, LicenseSeatEstimateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LicenseSeatEstimateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LicenseSeatEstimateStatus next)
    {
        switch (Status)
        {
            case LicenseSeatEstimateStatus.Open:
                return next == LicenseSeatEstimateStatus.InReview || next == LicenseSeatEstimateStatus.Reopened;
            case LicenseSeatEstimateStatus.InReview:
                switch (next)
                {
                    case LicenseSeatEstimateStatus.Reopened:
                    case LicenseSeatEstimateStatus.Archived:
                        return true;
                }
                return false;
            case LicenseSeatEstimateStatus.Resolved:
                switch (next)
                {
                    case LicenseSeatEstimateStatus.Reopened:
                        return true;
                }
                return false;
            case LicenseSeatEstimateStatus.Reopened:
                return next == LicenseSeatEstimateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(LicenseSeatEstimateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"license seat estimate {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Title}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)EndsAt - (double)DurationMinutes, 2);
    }

    public LicenseSeatEstimate Copy()
    {
        return new LicenseSeatEstimate(
            Code,
            Title,
            Region,
            Checksum,
            Label,
            JobCode,
            DurationMinutes,
            FloorValue,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
