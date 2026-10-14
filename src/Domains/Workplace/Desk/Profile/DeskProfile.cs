namespace Acme.Workforce.Workplace.Desk.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk profile is in its life cycle.</summary>
public enum DeskProfileStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One desk profile, with what the planners need to act on it.</summary>
public sealed class DeskProfile(string code, string name, int retries, int priority, double ratio, int updatedAt, bool visible, string checksum, DeskProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public int Priority { get; set; } = priority;
    public double Ratio { get; set; } = ratio;
    public int UpdatedAt { get; set; } = updatedAt;
    public bool Visible { get; set; } = visible;
    public string Checksum { get; set; } = checksum;
    public DeskProfileStatus Status { get; set; } = status;

    /// <summary>Starts a desk profile in status planned.</summary>
    public static DeskProfile NewDeskProfile(string code, string name)
    {
        return new DeskProfile(code, name, 0, 0, 0.0, 0, false, "", DeskProfileStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskProfileStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskProfileStatus next)
    {
        switch (Status)
        {
            case DeskProfileStatus.Planned:
                switch (next)
                {
                    case DeskProfileStatus.Scheduled:
                    case DeskProfileStatus.Cancelled:
                        return true;
                }
                return false;
            case DeskProfileStatus.Scheduled:
                return next == DeskProfileStatus.Cancelled;
            case DeskProfileStatus.InProgress:
                return next == DeskProfileStatus.Cancelled;
            case DeskProfileStatus.Completed:
                switch (next)
                {
                    case DeskProfileStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DeskProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedExposure()
    {
        return Math.Round((double)Priority - (double)UpdatedAt, 2);
    }

    public DeskProfile Copy()
    {
        return new DeskProfile(Code, Name, Retries, Priority, Ratio, UpdatedAt, Visible, Checksum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
