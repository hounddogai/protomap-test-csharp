namespace Acme.Workforce.Talent.CareerPath.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path assignment is in its life cycle.</summary>
public enum CareerPathAssignmentStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A career path assignment. Its code is unique within a tenant.</summary>
public sealed class CareerPathAssignment(string code, string name, double fte, string category, string checksum, int capacity, string title, int durationMinutes, string locale, bool approved, string country, CareerPathAssignmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Fte { get; set; } = fte;
    public string Category { get; set; } = category;
    public string Checksum { get; set; } = checksum;
    public int Capacity { get; set; } = capacity;
    public string Title { get; set; } = title;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Locale { get; set; } = locale;
    public bool Approved { get; set; } = approved;
    public string Country { get; set; } = country;
    public CareerPathAssignmentStatus Status { get; set; } = status;

    /// <summary>Starts a career path assignment in status idle.</summary>
    public static CareerPathAssignment NewCareerPathAssignment(string code, string name)
    {
        return new CareerPathAssignment(
            code,
            name,
            0.0,
            "",
            "",
            0,
            "",
            0,
            "",
            false,
            "",
            CareerPathAssignmentStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathAssignmentStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathAssignmentStatus next)
    {
        switch (Status)
        {
            case CareerPathAssignmentStatus.Idle:
                switch (next)
                {
                    case CareerPathAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathAssignmentStatus.Warming:
                switch (next)
                {
                    case CareerPathAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathAssignmentStatus.Ready:
                switch (next)
                {
                    case CareerPathAssignmentStatus.Draining:
                        return true;
                }
                return false;
            case CareerPathAssignmentStatus.Draining:
                switch (next)
                {
                    case CareerPathAssignmentStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathAssignmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path assignment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)DurationMinutes - (double)Capacity, 2);
    }

    public CareerPathAssignment Copy()
    {
        return new CareerPathAssignment(
            Code,
            Name,
            Fte,
            Category,
            Checksum,
            Capacity,
            Title,
            DurationMinutes,
            Locale,
            Approved,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
