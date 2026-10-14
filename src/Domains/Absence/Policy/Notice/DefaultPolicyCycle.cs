namespace Acme.Workforce.Absence.Policy.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default policy cycle is in its life cycle.</summary>
public enum DefaultPolicyCycleStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A default policy cycle as the policy team keeps it.</summary>
public sealed class DefaultPolicyCycle(string code, string title, int updatedAt, string team, int priority, double fte, string segment, double margin, DefaultPolicyCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int UpdatedAt { get; set; } = updatedAt;
    public string Team { get; set; } = team;
    public int Priority { get; set; } = priority;
    public double Fte { get; set; } = fte;
    public string Segment { get; set; } = segment;
    public double Margin { get; set; } = margin;
    public DefaultPolicyCycleStatus Status { get; set; } = status;

    /// <summary>Starts a default policy cycle in status planned.</summary>
    public static DefaultPolicyCycle NewDefaultPolicyCycle(string code, string title)
    {
        return new DefaultPolicyCycle(code, title, 0, "", 0, 0.0, "", 0.0, DefaultPolicyCycleStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPolicyCycleStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPolicyCycleStatus next)
    {
        switch (Status)
        {
            case DefaultPolicyCycleStatus.Planned:
                switch (next)
                {
                    case DefaultPolicyCycleStatus.Scheduled:
                    case DefaultPolicyCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultPolicyCycleStatus.Scheduled:
                return next == DefaultPolicyCycleStatus.InProgress || next == DefaultPolicyCycleStatus.Completed;
            case DefaultPolicyCycleStatus.InProgress:
                switch (next)
                {
                    case DefaultPolicyCycleStatus.Completed:
                    case DefaultPolicyCycleStatus.Cancelled:
                        return true;
                }
                return false;
            case DefaultPolicyCycleStatus.Completed:
                switch (next)
                {
                    case DefaultPolicyCycleStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultPolicyCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default policy cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default policy cycle"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)Priority - Fte, 2);
    }

    public DefaultPolicyCycle Copy()
    {
        return new DefaultPolicyCycle(Code, Title, UpdatedAt, Team, Priority, Fte, Segment, Margin, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
