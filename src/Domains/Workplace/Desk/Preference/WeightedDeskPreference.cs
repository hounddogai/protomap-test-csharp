namespace Acme.Workforce.Workplace.Desk.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted desk preference is in its life cycle.</summary>
public enum WeightedDeskPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A weighted desk preference as the desk team keeps it.</summary>
public sealed class WeightedDeskPreference(string code, string label, string salaryBand, string region, string department, string ownerId, int tenureMonths, int sequence, double weight, string timezone, WeightedDeskPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string SalaryBand { get; set; } = salaryBand;
    public string Region { get; set; } = region;
    public string Department { get; set; } = department;
    public string OwnerId { get; set; } = ownerId;
    public int TenureMonths { get; set; } = tenureMonths;
    public int Sequence { get; set; } = sequence;
    public double Weight { get; set; } = weight;
    public string Timezone { get; set; } = timezone;
    public WeightedDeskPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a weighted desk preference in status planned.</summary>
    public static WeightedDeskPreference NewWeightedDeskPreference(string code, string label)
    {
        return new WeightedDeskPreference(
            code,
            label,
            "",
            "",
            "",
            "",
            0,
            0,
            0.0,
            "",
            WeightedDeskPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedDeskPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedDeskPreferenceStatus next)
    {
        switch (Status)
        {
            case WeightedDeskPreferenceStatus.Planned:
                switch (next)
                {
                    case WeightedDeskPreferenceStatus.Scheduled:
                    case WeightedDeskPreferenceStatus.InProgress:
                        return true;
                }
                return false;
            case WeightedDeskPreferenceStatus.Scheduled:
                return next == WeightedDeskPreferenceStatus.InProgress;
            case WeightedDeskPreferenceStatus.InProgress:
                switch (next)
                {
                    case WeightedDeskPreferenceStatus.Completed:
                        return true;
                }
                return false;
            case WeightedDeskPreferenceStatus.Completed:
                switch (next)
                {
                    case WeightedDeskPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedDeskPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted desk preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double EffectiveVolume()
    {
        return Math.Round(Weight * (double)Sequence, 2);
    }

    public WeightedDeskPreference Copy()
    {
        return new WeightedDeskPreference(
            Code,
            Label,
            SalaryBand,
            Region,
            Department,
            OwnerId,
            TenureMonths,
            Sequence,
            Weight,
            Timezone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
