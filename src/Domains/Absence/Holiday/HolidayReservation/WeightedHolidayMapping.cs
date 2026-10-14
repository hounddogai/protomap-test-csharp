namespace Acme.Workforce.Absence.Holiday.HolidayReservation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted holiday mapping is in its life cycle.</summary>
public enum WeightedHolidayMappingStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A weighted holiday mapping. Its code is unique within a tenant.</summary>
public sealed class WeightedHolidayMapping(string code, string name, int limit, bool approved, string phone, int tenureMonths, int leaveDays, string skill, string contactEmail, WeightedHolidayMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Limit { get; set; } = limit;
    public bool Approved { get; set; } = approved;
    public string Phone { get; set; } = phone;
    public int TenureMonths { get; set; } = tenureMonths;
    public int LeaveDays { get; set; } = leaveDays;
    public string Skill { get; set; } = skill;
    public string ContactEmail { get; set; } = contactEmail;
    public WeightedHolidayMappingStatus Status { get; set; } = status;

    /// <summary>Starts a weighted holiday mapping in status idle.</summary>
    public static WeightedHolidayMapping NewWeightedHolidayMapping(string code, string name)
    {
        return new WeightedHolidayMapping(code, name, 0, false, "", 0, 0, "", "", WeightedHolidayMappingStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedHolidayMappingStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedHolidayMappingStatus next)
    {
        switch (Status)
        {
            case WeightedHolidayMappingStatus.Idle:
                switch (next)
                {
                    case WeightedHolidayMappingStatus.Warming:
                    case WeightedHolidayMappingStatus.Stopped:
                        return true;
                }
                return false;
            case WeightedHolidayMappingStatus.Warming:
                switch (next)
                {
                    case WeightedHolidayMappingStatus.Ready:
                    case WeightedHolidayMappingStatus.Draining:
                        return true;
                }
                return false;
            case WeightedHolidayMappingStatus.Ready:
                switch (next)
                {
                    case WeightedHolidayMappingStatus.Draining:
                        return true;
                }
                return false;
            case WeightedHolidayMappingStatus.Draining:
                return next == WeightedHolidayMappingStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(WeightedHolidayMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted holiday mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"weighted holiday mapping"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)LeaveDays - (double)Limit, 2);
    }

    public WeightedHolidayMapping Copy()
    {
        return new WeightedHolidayMapping(
            Code,
            Name,
            Limit,
            Approved,
            Phone,
            TenureMonths,
            LeaveDays,
            Skill,
            ContactEmail,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
