namespace Acme.Workforce.Assets.Assignment.Rule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a assignment forecast is in its life cycle.</summary>
public enum AssignmentForecastStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>One assignment forecast, with what the planners need to act on it.</summary>
public sealed class AssignmentForecast(string code, string title, string managerId, int graceDays, double cost, AssignmentForecastStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ManagerId { get; set; } = managerId;
    public int GraceDays { get; set; } = graceDays;
    public double Cost { get; set; } = cost;
    public AssignmentForecastStatus Status { get; set; } = status;

    /// <summary>Starts a assignment forecast in status proposed.</summary>
    public static AssignmentForecast NewAssignmentForecast(string code, string title)
    {
        return new AssignmentForecast(code, title, "", 0, 0.0, AssignmentForecastStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssignmentForecastStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssignmentForecastStatus next)
    {
        switch (Status)
        {
            case AssignmentForecastStatus.Proposed:
                switch (next)
                {
                    case AssignmentForecastStatus.Approved:
                    case AssignmentForecastStatus.Withdrawn:
                        return true;
                }
                return false;
            case AssignmentForecastStatus.Approved:
                return next == AssignmentForecastStatus.Rejected;
            case AssignmentForecastStatus.Rejected:
                switch (next)
                {
                    case AssignmentForecastStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssignmentForecastStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"assignment forecast {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"assignment forecast"} {Code}";
    }

    public double NetExposure()
    {
        return Math.Round((double)GraceDays - Cost, 2);
    }

    public AssignmentForecast Copy()
    {
        return new AssignmentForecast(Code, Title, ManagerId, GraceDays, Cost, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
