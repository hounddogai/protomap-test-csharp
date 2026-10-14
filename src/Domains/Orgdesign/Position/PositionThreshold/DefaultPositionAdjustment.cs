namespace Acme.Workforce.Orgdesign.Position.PositionThreshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default position adjustment is in its life cycle.</summary>
public enum DefaultPositionAdjustmentStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One default position adjustment, with what the planners need to act on it.</summary>
public sealed class DefaultPositionAdjustment(string code, string title, string ownerId, string jobCode, int windowDays, int updatedAt, int limit, double minimum, string name, DefaultPositionAdjustmentStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string OwnerId { get; set; } = ownerId;
    public string JobCode { get; set; } = jobCode;
    public int WindowDays { get; set; } = windowDays;
    public int UpdatedAt { get; set; } = updatedAt;
    public int Limit { get; set; } = limit;
    public double Minimum { get; set; } = minimum;
    public string Name { get; set; } = name;
    public DefaultPositionAdjustmentStatus Status { get; set; } = status;

    /// <summary>Starts a default position adjustment in status proposed.</summary>
    public static DefaultPositionAdjustment NewDefaultPositionAdjustment(string code, string title)
    {
        return new DefaultPositionAdjustment(
            code,
            title,
            "",
            "",
            0,
            0,
            0,
            0.0,
            "",
            DefaultPositionAdjustmentStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultPositionAdjustmentStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultPositionAdjustmentStatus next)
    {
        switch (Status)
        {
            case DefaultPositionAdjustmentStatus.Proposed:
                switch (next)
                {
                    case DefaultPositionAdjustmentStatus.Rejected:
                        return true;
                }
                return false;
            case DefaultPositionAdjustmentStatus.Approved:
                switch (next)
                {
                    case DefaultPositionAdjustmentStatus.Rejected:
                    case DefaultPositionAdjustmentStatus.Withdrawn:
                        return true;
                }
                return false;
            case DefaultPositionAdjustmentStatus.Rejected:
                return next == DefaultPositionAdjustmentStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(DefaultPositionAdjustmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default position adjustment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"default position adjustment"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)UpdatedAt - Minimum, 2);
    }

    public DefaultPositionAdjustment Copy()
    {
        return new DefaultPositionAdjustment(
            Code,
            Title,
            OwnerId,
            JobCode,
            WindowDays,
            UpdatedAt,
            Limit,
            Minimum,
            Name,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
