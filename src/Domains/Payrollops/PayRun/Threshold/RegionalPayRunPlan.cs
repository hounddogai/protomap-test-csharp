namespace Acme.Workforce.Payrollops.PayRun.Threshold;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional pay run plan is in its life cycle.</summary>
public enum RegionalPayRunPlanStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>A regional pay run plan. Its code is unique within a tenant.</summary>
public sealed class RegionalPayRunPlan(string code, string title, double minimum, string source, string summary, int durationMinutes, string skill, double rating, RegionalPayRunPlanStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Minimum { get; set; } = minimum;
    public string Source { get; set; } = source;
    public string Summary { get; set; } = summary;
    public int DurationMinutes { get; set; } = durationMinutes;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public RegionalPayRunPlanStatus Status { get; set; } = status;

    /// <summary>Starts a regional pay run plan in status idle.</summary>
    public static RegionalPayRunPlan NewRegionalPayRunPlan(string code, string title)
    {
        return new RegionalPayRunPlan(code, title, 0.0, "", "", 0, "", 0.0, RegionalPayRunPlanStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalPayRunPlanStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalPayRunPlanStatus next)
    {
        switch (Status)
        {
            case RegionalPayRunPlanStatus.Idle:
                switch (next)
                {
                    case RegionalPayRunPlanStatus.Draining:
                        return true;
                }
                return false;
            case RegionalPayRunPlanStatus.Warming:
                switch (next)
                {
                    case RegionalPayRunPlanStatus.Draining:
                    case RegionalPayRunPlanStatus.Stopped:
                        return true;
                }
                return false;
            case RegionalPayRunPlanStatus.Ready:
                switch (next)
                {
                    case RegionalPayRunPlanStatus.Draining:
                        return true;
                }
                return false;
            case RegionalPayRunPlanStatus.Draining:
                switch (next)
                {
                    case RegionalPayRunPlanStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalPayRunPlanStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional pay run plan {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"regional pay run plan"} {Code}";
    }

    public double AdjustedVolume()
    {
        return Math.Round((double)DurationMinutes * Rating, 2);
    }

    public RegionalPayRunPlan Copy()
    {
        return new RegionalPayRunPlan(Code, Title, Minimum, Source, Summary, DurationMinutes, Skill, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
