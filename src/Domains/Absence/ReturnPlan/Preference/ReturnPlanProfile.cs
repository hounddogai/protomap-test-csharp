namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan profile is in its life cycle.</summary>
public enum ReturnPlanProfileStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One return plan profile, with what the planners need to act on it.</summary>
public sealed class ReturnPlanProfile(string code, string name, double floorValue, string source, int windowDays, ReturnPlanProfileStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double FloorValue { get; set; } = floorValue;
    public string Source { get; set; } = source;
    public int WindowDays { get; set; } = windowDays;
    public ReturnPlanProfileStatus Status { get; set; } = status;

    /// <summary>Starts a return plan profile in status idle.</summary>
    public static ReturnPlanProfile NewReturnPlanProfile(string code, string name)
    {
        return new ReturnPlanProfile(code, name, 0.0, "", 0, ReturnPlanProfileStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanProfileStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanProfileStatus next)
    {
        switch (Status)
        {
            case ReturnPlanProfileStatus.Idle:
                switch (next)
                {
                    case ReturnPlanProfileStatus.Stopped:
                        return true;
                }
                return false;
            case ReturnPlanProfileStatus.Warming:
                return next == ReturnPlanProfileStatus.Ready;
            case ReturnPlanProfileStatus.Ready:
                return next == ReturnPlanProfileStatus.Draining;
            case ReturnPlanProfileStatus.Draining:
                return next == ReturnPlanProfileStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReturnPlanProfileStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan profile {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public double NetVolume()
    {
        return Math.Round(FloorValue + (double)WindowDays, 2);
    }

    public ReturnPlanProfile Copy()
    {
        return new ReturnPlanProfile(Code, Name, FloorValue, Source, WindowDays, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
