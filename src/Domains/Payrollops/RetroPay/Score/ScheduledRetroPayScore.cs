namespace Acme.Workforce.Payrollops.RetroPay.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled retro pay score is in its life cycle.</summary>
public enum ScheduledRetroPayScoreStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A scheduled retro pay score as the retro pay team keeps it.</summary>
public sealed class ScheduledRetroPayScore(string code, string title, string grade, string managerId, string unit, string ownerId, int graceDays, string region, ScheduledRetroPayScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public string ManagerId { get; set; } = managerId;
    public string Unit { get; set; } = unit;
    public string OwnerId { get; set; } = ownerId;
    public int GraceDays { get; set; } = graceDays;
    public string Region { get; set; } = region;
    public ScheduledRetroPayScoreStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled retro pay score in status draft.</summary>
    public static ScheduledRetroPayScore NewScheduledRetroPayScore(string code, string title)
    {
        return new ScheduledRetroPayScore(code, title, "", "", "", "", 0, "", ScheduledRetroPayScoreStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRetroPayScoreStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRetroPayScoreStatus next)
    {
        switch (Status)
        {
            case ScheduledRetroPayScoreStatus.Draft:
                return next == ScheduledRetroPayScoreStatus.Suspended;
            case ScheduledRetroPayScoreStatus.Pending:
                return next == ScheduledRetroPayScoreStatus.Active || next == ScheduledRetroPayScoreStatus.Closed;
            case ScheduledRetroPayScoreStatus.Active:
                return next == ScheduledRetroPayScoreStatus.Closed;
            case ScheduledRetroPayScoreStatus.Suspended:
                switch (next)
                {
                    case ScheduledRetroPayScoreStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledRetroPayScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled retro pay score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled retro pay score"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)GraceDays;
    }

    public ScheduledRetroPayScore Copy()
    {
        return new ScheduledRetroPayScore(Code, Title, Grade, ManagerId, Unit, OwnerId, GraceDays, Region, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
