namespace Acme.Workforce.Absence.Holiday.HolidayQuota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a lenient holiday cycle is in its life cycle.</summary>
public enum LenientHolidayCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One lenient holiday cycle, with what the planners need to act on it.</summary>
public sealed class LenientHolidayCycle(string code, string name, double maximum, string team, double amount, LenientHolidayCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public string Team { get; set; } = team;
    public double Amount { get; set; } = amount;
    public LenientHolidayCycleStatus Status { get; set; } = status;

    /// <summary>Starts a lenient holiday cycle in status created.</summary>
    public static LenientHolidayCycle NewLenientHolidayCycle(string code, string name)
    {
        return new LenientHolidayCycle(code, name, 0.0, "", 0.0, LenientHolidayCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case LenientHolidayCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(LenientHolidayCycleStatus next)
    {
        switch (Status)
        {
            case LenientHolidayCycleStatus.Created:
                switch (next)
                {
                    case LenientHolidayCycleStatus.Validated:
                    case LenientHolidayCycleStatus.Published:
                        return true;
                }
                return false;
            case LenientHolidayCycleStatus.Validated:
                switch (next)
                {
                    case LenientHolidayCycleStatus.Published:
                    case LenientHolidayCycleStatus.Retired:
                        return true;
                }
                return false;
            case LenientHolidayCycleStatus.Published:
                switch (next)
                {
                    case LenientHolidayCycleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(LenientHolidayCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"lenient holiday cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Maximum - Amount, 2);
    }

    public LenientHolidayCycle Copy()
    {
        return new LenientHolidayCycle(Code, Name, Maximum, Team, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
