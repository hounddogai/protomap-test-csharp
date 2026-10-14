namespace Acme.Workforce.Compliance.Certification.CertificationForecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual certification plan is in its life cycle.</summary>
public enum ManualCertificationPlanStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual certification plan as the certification team keeps it.</summary>
public sealed class ManualCertificationPlan(string code, string name, string location, int limit, string locale, double cost, string segment, bool approved, int startsAt, int windowDays, int version, ManualCertificationPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Location { get; set; } = location;
    public int Limit { get; set; } = limit;
    public string Locale { get; set; } = locale;
    public double Cost { get; set; } = cost;
    public string Segment { get; set; } = segment;
    public bool Approved { get; set; } = approved;
    public int StartsAt { get; set; } = startsAt;
    public int WindowDays { get; set; } = windowDays;
    public int Version { get; set; } = version;
    public ManualCertificationPlanStatus Status { get; set; } = status;

    /// <summary>Starts a manual certification plan in status created.</summary>
    public static ManualCertificationPlan NewManualCertificationPlan(string code, string name)
    {
        return new ManualCertificationPlan(
            code,
            name,
            "",
            0,
            "",
            0.0,
            "",
            false,
            0,
            0,
            0,
            ManualCertificationPlanStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualCertificationPlanStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualCertificationPlanStatus next)
    {
        switch (Status)
        {
            case ManualCertificationPlanStatus.Created:
                return next == ManualCertificationPlanStatus.Retired;
            case ManualCertificationPlanStatus.Validated:
                return next == ManualCertificationPlanStatus.Retired;
            case ManualCertificationPlanStatus.Published:
                switch (next)
                {
                    case ManualCertificationPlanStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ManualCertificationPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual certification plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)WindowDays * (double)Limit, 2);
    }

    public ManualCertificationPlan Copy()
    {
        return new ManualCertificationPlan(
            Code,
            Name,
            Location,
            Limit,
            Locale,
            Cost,
            Segment,
            Approved,
            StartsAt,
            WindowDays,
            Version,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
