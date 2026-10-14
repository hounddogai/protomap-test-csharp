namespace Acme.Workforce.Talent.CareerPath.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path segment is in its life cycle.</summary>
public enum CareerPathSegmentStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A career path segment as the career path team keeps it.</summary>
public sealed class CareerPathSegment(string code, string name, int revision, string costCenter, string team, string grade, double ceiling, Dictionary<string, string> attributes, int startsAt, CareerPathSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Revision { get; set; } = revision;
    public string CostCenter { get; set; } = costCenter;
    public string Team { get; set; } = team;
    public string Grade { get; set; } = grade;
    public double Ceiling { get; set; } = ceiling;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int StartsAt { get; set; } = startsAt;
    public CareerPathSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a career path segment in status planned.</summary>
    public static CareerPathSegment NewCareerPathSegment(string code, string name)
    {
        return new CareerPathSegment(
            code,
            name,
            0,
            "",
            "",
            "",
            0.0,
            new Dictionary<string, string>(),
            0,
            CareerPathSegmentStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathSegmentStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathSegmentStatus next)
    {
        switch (Status)
        {
            case CareerPathSegmentStatus.Planned:
                switch (next)
                {
                    case CareerPathSegmentStatus.Completed:
                    case CareerPathSegmentStatus.Cancelled:
                        return true;
                }
                return false;
            case CareerPathSegmentStatus.Scheduled:
                return next == CareerPathSegmentStatus.InProgress;
            case CareerPathSegmentStatus.InProgress:
                return next == CareerPathSegmentStatus.Completed;
            case CareerPathSegmentStatus.Completed:
                return next == CareerPathSegmentStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(CareerPathSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"career path segment"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Revision + (double)StartsAt, 2);
    }

    public CareerPathSegment Copy()
    {
        return new CareerPathSegment(
            Code,
            Name,
            Revision,
            CostCenter,
            Team,
            Grade,
            Ceiling,
            Attributes,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
