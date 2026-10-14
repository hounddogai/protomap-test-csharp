namespace Acme.Workforce.Talent.CareerPath.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path exception is in its life cycle.</summary>
public enum CareerPathExceptionStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a career path exception and its current status.</summary>
public sealed class CareerPathException(string code, string title, int expiresAt, int startsAt, int durationMinutes, double floorValue, double rate, int updatedAt, string reviewer, string country, CareerPathExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int ExpiresAt { get; set; } = expiresAt;
    public int StartsAt { get; set; } = startsAt;
    public int DurationMinutes { get; set; } = durationMinutes;
    public double FloorValue { get; set; } = floorValue;
    public double Rate { get; set; } = rate;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Reviewer { get; set; } = reviewer;
    public string Country { get; set; } = country;
    public CareerPathExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a career path exception in status proposed.</summary>
    public static CareerPathException NewCareerPathException(string code, string title)
    {
        return new CareerPathException(
            code,
            title,
            0,
            0,
            0,
            0.0,
            0.0,
            0,
            "",
            "",
            CareerPathExceptionStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathExceptionStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathExceptionStatus next)
    {
        switch (Status)
        {
            case CareerPathExceptionStatus.Proposed:
                return next == CareerPathExceptionStatus.Rejected || next == CareerPathExceptionStatus.Withdrawn;
            case CareerPathExceptionStatus.Approved:
                switch (next)
                {
                    case CareerPathExceptionStatus.Rejected:
                    case CareerPathExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
            case CareerPathExceptionStatus.Rejected:
                switch (next)
                {
                    case CareerPathExceptionStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"career path exception"} {Code}";
    }

    public double ProjectedExposure()
    {
        return Math.Round((double)DurationMinutes + (double)UpdatedAt, 2);
    }

    public CareerPathException Copy()
    {
        return new CareerPathException(
            Code,
            Title,
            ExpiresAt,
            StartsAt,
            DurationMinutes,
            FloorValue,
            Rate,
            UpdatedAt,
            Reviewer,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
