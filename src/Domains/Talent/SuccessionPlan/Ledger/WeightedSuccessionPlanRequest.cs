namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted succession plan request is in its life cycle.</summary>
public enum WeightedSuccessionPlanRequestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>One weighted succession plan request, with what the planners need to act on it.</summary>
public sealed class WeightedSuccessionPlanRequest(string code, string label, string team, string jobCode, int priority, WeightedSuccessionPlanRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public string JobCode { get; set; } = jobCode;
    public int Priority { get; set; } = priority;
    public WeightedSuccessionPlanRequestStatus Status { get; set; } = status;

    /// <summary>Starts a weighted succession plan request in status new.</summary>
    public static WeightedSuccessionPlanRequest NewWeightedSuccessionPlanRequest(string code, string label)
    {
        return new WeightedSuccessionPlanRequest(code, label, "", "", 0, WeightedSuccessionPlanRequestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedSuccessionPlanRequestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedSuccessionPlanRequestStatus next)
    {
        switch (Status)
        {
            case WeightedSuccessionPlanRequestStatus.New:
                return next == WeightedSuccessionPlanRequestStatus.Queued;
            case WeightedSuccessionPlanRequestStatus.Queued:
                switch (next)
                {
                    case WeightedSuccessionPlanRequestStatus.Failed:
                        return true;
                }
                return false;
            case WeightedSuccessionPlanRequestStatus.Running:
                return next == WeightedSuccessionPlanRequestStatus.Done;
            case WeightedSuccessionPlanRequestStatus.Done:
                switch (next)
                {
                    case WeightedSuccessionPlanRequestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WeightedSuccessionPlanRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted succession plan request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"weighted succession plan request"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)Priority;
    }

    public WeightedSuccessionPlanRequest Copy()
    {
        return new WeightedSuccessionPlanRequest(Code, Label, Team, JobCode, Priority, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
