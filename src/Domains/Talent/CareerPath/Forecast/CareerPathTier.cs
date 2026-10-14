namespace Acme.Workforce.Talent.CareerPath.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path tier is in its life cycle.</summary>
public enum CareerPathTierStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One career path tier, with what the planners need to act on it.</summary>
public sealed class CareerPathTier(string code, string name, int version, bool archived, string locale, double maximum, int graceDays, string ownerId, CareerPathTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Version { get; set; } = version;
    public bool Archived { get; set; } = archived;
    public string Locale { get; set; } = locale;
    public double Maximum { get; set; } = maximum;
    public int GraceDays { get; set; } = graceDays;
    public string OwnerId { get; set; } = ownerId;
    public CareerPathTierStatus Status { get; set; } = status;

    /// <summary>Starts a career path tier in status new.</summary>
    public static CareerPathTier NewCareerPathTier(string code, string name)
    {
        return new CareerPathTier(code, name, 0, false, "", 0.0, 0, "", CareerPathTierStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathTierStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathTierStatus next)
    {
        switch (Status)
        {
            case CareerPathTierStatus.New:
                switch (next)
                {
                    case CareerPathTierStatus.Queued:
                        return true;
                }
                return false;
            case CareerPathTierStatus.Queued:
                switch (next)
                {
                    case CareerPathTierStatus.Running:
                        return true;
                }
                return false;
            case CareerPathTierStatus.Running:
                switch (next)
                {
                    case CareerPathTierStatus.Done:
                        return true;
                }
                return false;
            case CareerPathTierStatus.Done:
                switch (next)
                {
                    case CareerPathTierStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{"career path tier"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Version * (double)GraceDays, 2);
    }

    public CareerPathTier Copy()
    {
        return new CareerPathTier(Code, Name, Version, Archived, Locale, Maximum, GraceDays, OwnerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
