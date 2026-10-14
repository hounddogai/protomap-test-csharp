namespace Acme.Workforce.Absence.Holiday.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal holiday claim is in its life cycle.</summary>
public enum SeasonalHolidayClaimStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A seasonal holiday claim as the holiday team keeps it.</summary>
public sealed class SeasonalHolidayClaim(string code, string name, string segment, double rating, int priority, string managerId, string salaryBand, int expiresAt, double hours, SeasonalHolidayClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Rating { get; set; } = rating;
    public int Priority { get; set; } = priority;
    public string ManagerId { get; set; } = managerId;
    public string SalaryBand { get; set; } = salaryBand;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Hours { get; set; } = hours;
    public SeasonalHolidayClaimStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal holiday claim in status created.</summary>
    public static SeasonalHolidayClaim NewSeasonalHolidayClaim(string code, string name)
    {
        return new SeasonalHolidayClaim(code, name, "", 0.0, 0, "", "", 0, 0.0, SeasonalHolidayClaimStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalHolidayClaimStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalHolidayClaimStatus next)
    {
        switch (Status)
        {
            case SeasonalHolidayClaimStatus.Created:
                return next == SeasonalHolidayClaimStatus.Published;
            case SeasonalHolidayClaimStatus.Validated:
                switch (next)
                {
                    case SeasonalHolidayClaimStatus.Published:
                    case SeasonalHolidayClaimStatus.Retired:
                        return true;
                }
                return false;
            case SeasonalHolidayClaimStatus.Published:
                switch (next)
                {
                    case SeasonalHolidayClaimStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalHolidayClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal holiday claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"seasonal holiday claim"} {Code}";
    }

    public double EffectiveValue()
    {
        return Math.Round((double)Priority * (double)ExpiresAt, 2);
    }

    public SeasonalHolidayClaim Copy()
    {
        return new SeasonalHolidayClaim(
            Code,
            Name,
            Segment,
            Rating,
            Priority,
            ManagerId,
            SalaryBand,
            ExpiresAt,
            Hours,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
