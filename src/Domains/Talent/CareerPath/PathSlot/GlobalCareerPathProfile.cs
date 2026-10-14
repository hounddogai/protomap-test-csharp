namespace Acme.Workforce.Talent.CareerPath.PathSlot;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global career path profile is in its life cycle.</summary>
public enum GlobalCareerPathProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A global career path profile as the career path team keeps it.</summary>
public sealed class GlobalCareerPathProfile(string code, string title, int capacity, string name, double maximum, int expiresAt, int attempts, string jobCode, bool visible, GlobalCareerPathProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Capacity { get; set; } = capacity;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int ExpiresAt { get; set; } = expiresAt;
    public int Attempts { get; set; } = attempts;
    public string JobCode { get; set; } = jobCode;
    public bool Visible { get; set; } = visible;
    public GlobalCareerPathProfileStatus Status { get; set; } = status;

    /// <summary>Starts a global career path profile in status planned.</summary>
    public static GlobalCareerPathProfile NewGlobalCareerPathProfile(string code, string title)
    {
        return new GlobalCareerPathProfile(
            code,
            title,
            0,
            "",
            0.0,
            0,
            0,
            "",
            false,
            GlobalCareerPathProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalCareerPathProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalCareerPathProfileStatus next)
    {
        switch (Status)
        {
            case GlobalCareerPathProfileStatus.Planned:
                switch (next)
                {
                    case GlobalCareerPathProfileStatus.Scheduled:
                    case GlobalCareerPathProfileStatus.InProgress:
                        return true;
                }
                return false;
            case GlobalCareerPathProfileStatus.Scheduled:
                return next == GlobalCareerPathProfileStatus.Cancelled;
            case GlobalCareerPathProfileStatus.InProgress:
                return next == GlobalCareerPathProfileStatus.Completed;
            case GlobalCareerPathProfileStatus.Completed:
                return next == GlobalCareerPathProfileStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(GlobalCareerPathProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global career path profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"global career path profile"} {Code}";
    }

    public double NetLoad()
    {
        return Math.Round(Maximum + (double)Attempts, 2);
    }

    public GlobalCareerPathProfile Copy()
    {
        return new GlobalCareerPathProfile(
            Code,
            Title,
            Capacity,
            Name,
            Maximum,
            ExpiresAt,
            Attempts,
            JobCode,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
