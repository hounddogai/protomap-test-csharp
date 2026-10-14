namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a adaptive holiday claim is in its life cycle.</summary>
public enum AdaptiveHolidayClaimStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a adaptive holiday claim and its current status.</summary>
public sealed class AdaptiveHolidayClaim(string code, string name, double amount, string phone, string employeeId, string country, int attempts, double rate, AdaptiveHolidayClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string Phone { get; set; } = phone;
    public string EmployeeId { get; set; } = employeeId;
    public string Country { get; set; } = country;
    public int Attempts { get; set; } = attempts;
    public double Rate { get; set; } = rate;
    public AdaptiveHolidayClaimStatus Status { get; set; } = status;

    /// <summary>Starts a adaptive holiday claim in status new.</summary>
    public static AdaptiveHolidayClaim NewAdaptiveHolidayClaim(string code, string name)
    {
        return new AdaptiveHolidayClaim(code, name, 0.0, "", "", "", 0, 0.0, AdaptiveHolidayClaimStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AdaptiveHolidayClaimStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AdaptiveHolidayClaimStatus next)
    {
        switch (Status)
        {
            case AdaptiveHolidayClaimStatus.New:
                return next == AdaptiveHolidayClaimStatus.Running || next == AdaptiveHolidayClaimStatus.Failed;
            case AdaptiveHolidayClaimStatus.Queued:
                return next == AdaptiveHolidayClaimStatus.Done;
            case AdaptiveHolidayClaimStatus.Running:
                switch (next)
                {
                    case AdaptiveHolidayClaimStatus.Done:
                    case AdaptiveHolidayClaimStatus.Failed:
                        return true;
                }
                return false;
            case AdaptiveHolidayClaimStatus.Done:
                switch (next)
                {
                    case AdaptiveHolidayClaimStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AdaptiveHolidayClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"adaptive holiday claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveExposure()
    {
        return Math.Round(Amount - (double)Attempts, 2);
    }

    public AdaptiveHolidayClaim Copy()
    {
        return new AdaptiveHolidayClaim(Code, Name, Amount, Phone, EmployeeId, Country, Attempts, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
