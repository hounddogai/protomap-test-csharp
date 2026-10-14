namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path preference is in its life cycle.</summary>
public enum CareerPathPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A career path preference. Its code is unique within a tenant.</summary>
public sealed class CareerPathPreference(string code, string title, int durationMinutes, int createdAt, double floorValue, string salaryBand, bool archived, string timezone, CareerPathPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public int CreatedAt { get; set; } = createdAt;
    public double FloorValue { get; set; } = floorValue;
    public string SalaryBand { get; set; } = salaryBand;
    public bool Archived { get; set; } = archived;
    public string Timezone { get; set; } = timezone;
    public CareerPathPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a career path preference in status planned.</summary>
    public static CareerPathPreference NewCareerPathPreference(string code, string title)
    {
        return new CareerPathPreference(code, title, 0, 0, 0.0, "", false, "", CareerPathPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathPreferenceStatus next)
    {
        switch (Status)
        {
            case CareerPathPreferenceStatus.Planned:
                return next == CareerPathPreferenceStatus.InProgress;
            case CareerPathPreferenceStatus.Scheduled:
                return next == CareerPathPreferenceStatus.InProgress || next == CareerPathPreferenceStatus.Cancelled;
            case CareerPathPreferenceStatus.InProgress:
                switch (next)
                {
                    case CareerPathPreferenceStatus.Completed:
                    case CareerPathPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case CareerPathPreferenceStatus.Completed:
                switch (next)
                {
                    case CareerPathPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)CreatedAt * FloorValue, 2);
    }

    public CareerPathPreference Copy()
    {
        return new CareerPathPreference(
            Code,
            Title,
            DurationMinutes,
            CreatedAt,
            FloorValue,
            SalaryBand,
            Archived,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
