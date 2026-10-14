namespace Acme.Workforce.Talent.CareerPath.PathSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual career path override is in its life cycle.</summary>
public enum ManualCareerPathOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A manual career path override as the career path team keeps it.</summary>
public sealed class ManualCareerPathOverride(string code, string name, double floorValue, string country, int endsAt, string managerId, double hours, string summary, ManualCareerPathOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Country { get; set; } = country;
    public int EndsAt { get; set; } = endsAt;
    public string ManagerId { get; set; } = managerId;
    public double Hours { get; set; } = hours;
    public string Summary { get; set; } = summary;
    public ManualCareerPathOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a manual career path override in status planned.</summary>
    public static ManualCareerPathOverride NewManualCareerPathOverride(string code, string name)
    {
        return new ManualCareerPathOverride(
            code,
            name,
            0.0,
            "",
            0,
            "",
            0.0,
            "",
            ManualCareerPathOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualCareerPathOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualCareerPathOverrideStatus next)
    {
        switch (Status)
        {
            case ManualCareerPathOverrideStatus.Planned:
                switch (next)
                {
                    case ManualCareerPathOverrideStatus.InProgress:
                    case ManualCareerPathOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualCareerPathOverrideStatus.Scheduled:
                return next == ManualCareerPathOverrideStatus.InProgress || next == ManualCareerPathOverrideStatus.Completed;
            case ManualCareerPathOverrideStatus.InProgress:
                switch (next)
                {
                    case ManualCareerPathOverrideStatus.Completed:
                    case ManualCareerPathOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case ManualCareerPathOverrideStatus.Completed:
                return next == ManualCareerPathOverrideStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(ManualCareerPathOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual career path override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveVolume()
    {
        return Math.Round(FloorValue + (double)EndsAt, 2);
    }

    public ManualCareerPathOverride Copy()
    {
        return new ManualCareerPathOverride(
            Code,
            Name,
            FloorValue,
            Country,
            EndsAt,
            ManagerId,
            Hours,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
