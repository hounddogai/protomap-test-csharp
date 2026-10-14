namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual certification forecast is in its life cycle.</summary>
public enum ManualCertificationForecastStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual certification forecast. Its code is unique within a tenant.</summary>
public sealed class ManualCertificationForecast(string code, string name, string salaryBand, int version, double fte, string externalRef, int endsAt, double margin, ManualCertificationForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public int Version { get; set; } = version;
    public double Fte { get; set; } = fte;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public double Margin { get; set; } = margin;
    public ManualCertificationForecastStatus Status { get; set; } = status;

    /// <summary>Starts a manual certification forecast in status planned.</summary>
    public static ManualCertificationForecast NewManualCertificationForecast(string code, string name)
    {
        return new ManualCertificationForecast(
            code,
            name,
            "",
            0,
            0.0,
            "",
            0,
            0.0,
            ManualCertificationForecastStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualCertificationForecastStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualCertificationForecastStatus next)
    {
        switch (Status)
        {
            case ManualCertificationForecastStatus.Planned:
                return next == ManualCertificationForecastStatus.Scheduled || next == ManualCertificationForecastStatus.Completed;
            case ManualCertificationForecastStatus.Scheduled:
                return next == ManualCertificationForecastStatus.InProgress || next == ManualCertificationForecastStatus.Cancelled;
            case ManualCertificationForecastStatus.InProgress:
                return next == ManualCertificationForecastStatus.Cancelled;
            case ManualCertificationForecastStatus.Completed:
                return next == ManualCertificationForecastStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualCertificationForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual certification forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{"manual certification forecast"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round(Fte + (double)EndsAt, 2);
    }

    public ManualCertificationForecast Copy()
    {
        return new ManualCertificationForecast(
            Code,
            Name,
            SalaryBand,
            Version,
            Fte,
            ExternalRef,
            EndsAt,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
