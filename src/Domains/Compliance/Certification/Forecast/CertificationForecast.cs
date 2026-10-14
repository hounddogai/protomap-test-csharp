namespace Acme.Workforce.Compliance.Certification.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification forecast is in its life cycle.</summary>
public enum CertificationForecastStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A certification forecast as the certification team keeps it.</summary>
public sealed class CertificationForecast(string code, string name, double amount, double floorValue, string skill, int priority, double rating, string ownerId, int tenureMonths, bool active, CertificationForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public double FloorValue { get; set; } = floorValue;
    public string Skill { get; set; } = skill;
    public int Priority { get; set; } = priority;
    public double Rating { get; set; } = rating;
    public string OwnerId { get; set; } = ownerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Active { get; set; } = active;
    public CertificationForecastStatus Status { get; set; } = status;

    /// <summary>Starts a certification forecast in status draft.</summary>
    public static CertificationForecast NewCertificationForecast(string code, string name)
    {
        return new CertificationForecast(
            code,
            name,
            0.0,
            0.0,
            "",
            0,
            0.0,
            "",
            0,
            false,
            CertificationForecastStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationForecastStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationForecastStatus next)
    {
        switch (Status)
        {
            case CertificationForecastStatus.Draft:
                return next == CertificationForecastStatus.Suspended;
            case CertificationForecastStatus.Pending:
                return next == CertificationForecastStatus.Suspended || next == CertificationForecastStatus.Closed;
            case CertificationForecastStatus.Active:
                return next == CertificationForecastStatus.Suspended || next == CertificationForecastStatus.Closed;
            case CertificationForecastStatus.Suspended:
                switch (next)
                {
                    case CertificationForecastStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"certification forecast"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Amount + Rating, 2);
    }

    public CertificationForecast Copy()
    {
        return new CertificationForecast(
            Code,
            Name,
            Amount,
            FloorValue,
            Skill,
            Priority,
            Rating,
            OwnerId,
            TenureMonths,
            Active,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
