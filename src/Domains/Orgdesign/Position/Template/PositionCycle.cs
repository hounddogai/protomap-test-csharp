namespace Acme.Workforce.Orgdesign.Position.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position cycle is in its life cycle.</summary>
public enum PositionCycleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A position cycle as the position team keeps it.</summary>
public sealed class PositionCycle(string code, string title, double ratio, double cost, string timezone, int durationMinutes, string contactEmail, string grade, double threshold, string costCenter, PositionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Cost { get; set; } = cost;
    public string Timezone { get; set; } = timezone;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string ContactEmail { get; set; } = contactEmail;
    public string Grade { get; set; } = grade;
    public double Threshold { get; set; } = threshold;
    public string CostCenter { get; set; } = costCenter;
    public PositionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a position cycle in status proposed.</summary>
    public static PositionCycle NewPositionCycle(string code, string title)
    {
        return new PositionCycle(code, title, 0.0, 0.0, "", 0, "", "", 0.0, "", PositionCycleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionCycleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionCycleStatus next)
    {
        switch (Status)
        {
            case PositionCycleStatus.Proposed:
                switch (next)
                {
                    case PositionCycleStatus.Approved:
                    case PositionCycleStatus.Rejected:
                        return true;
                }
                return false;
            case PositionCycleStatus.Approved:
                return next == PositionCycleStatus.Rejected || next == PositionCycleStatus.Withdrawn;
            case PositionCycleStatus.Rejected:
                switch (next)
                {
                    case PositionCycleStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PositionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)DurationMinutes - Cost, 2);
    }

    public PositionCycle Copy()
    {
        return new PositionCycle(
            Code,
            Title,
            Ratio,
            Cost,
            Timezone,
            DurationMinutes,
            ContactEmail,
            Grade,
            Threshold,
            CostCenter,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
