namespace Acme.Workforce.Assets.Laptop.Audit;

using Acme.Workforce.Kit.Text;

/// <summary>Where a rolling laptop exception is in its life cycle.</summary>
public enum RollingLaptopExceptionStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One rolling laptop exception, with what the planners need to act on it.</summary>
public sealed class RollingLaptopException(string code, string label, int startsAt, int sequence, double maximum, string name, string channel, int durationMinutes, string title, string department, int updatedAt, RollingLaptopExceptionStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public int Sequence { get; set; } = sequence;
    public double Maximum { get; set; } = maximum;
    public string Name { get; set; } = name;
    public string Channel { get; set; } = channel;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int UpdatedAt { get; set; } = updatedAt;
    public RollingLaptopExceptionStatus Status { get; set; } = status;

    /// <summary>Starts a rolling laptop exception in status open.</summary>
    public static RollingLaptopException NewRollingLaptopException(string code, string label)
    {
        return new RollingLaptopException(
            code,
            label,
            0,
            0,
            0.0,
            "",
            "",
            0,
            "",
            "",
            0,
            RollingLaptopExceptionStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RollingLaptopExceptionStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RollingLaptopExceptionStatus next)
    {
        switch (Status)
        {
            case RollingLaptopExceptionStatus.Open:
                switch (next)
                {
                    case RollingLaptopExceptionStatus.Reopened:
                        return true;
                }
                return false;
            case RollingLaptopExceptionStatus.InReview:
                return next == RollingLaptopExceptionStatus.Resolved || next == RollingLaptopExceptionStatus.Reopened;
            case RollingLaptopExceptionStatus.Resolved:
                switch (next)
                {
                    case RollingLaptopExceptionStatus.Reopened:
                        return true;
                }
                return false;
            case RollingLaptopExceptionStatus.Reopened:
                switch (next)
                {
                    case RollingLaptopExceptionStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RollingLaptopExceptionStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"rolling laptop exception {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"rolling laptop exception"} {Code}";
    }

    public double NetValue()
    {
        return Math.Round((double)DurationMinutes * Maximum, 2);
    }

    public RollingLaptopException Copy()
    {
        return new RollingLaptopException(
            Code,
            Label,
            StartsAt,
            Sequence,
            Maximum,
            Name,
            Channel,
            DurationMinutes,
            Title,
            Department,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
