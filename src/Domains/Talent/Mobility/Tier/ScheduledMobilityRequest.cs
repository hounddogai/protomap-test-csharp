namespace Acme.Workforce.Talent.Mobility.Tier;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled mobility request is in its life cycle.</summary>
public enum ScheduledMobilityRequestStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a scheduled mobility request and its current status.</summary>
public sealed class ScheduledMobilityRequest(string code, string name, string employeeId, bool approved, bool active, bool archived, double amount, ScheduledMobilityRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string EmployeeId { get; set; } = employeeId;
    public bool Approved { get; set; } = approved;
    public bool Active { get; set; } = active;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public ScheduledMobilityRequestStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled mobility request in status draft.</summary>
    public static ScheduledMobilityRequest NewScheduledMobilityRequest(string code, string name)
    {
        return new ScheduledMobilityRequest(
            code,
            name,
            "",
            false,
            false,
            false,
            0.0,
            ScheduledMobilityRequestStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledMobilityRequestStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledMobilityRequestStatus next)
    {
        switch (Status)
        {
            case ScheduledMobilityRequestStatus.Draft:
                switch (next)
                {
                    case ScheduledMobilityRequestStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledMobilityRequestStatus.Pending:
                return next == ScheduledMobilityRequestStatus.Suspended || next == ScheduledMobilityRequestStatus.Closed;
            case ScheduledMobilityRequestStatus.Active:
                switch (next)
                {
                    case ScheduledMobilityRequestStatus.Suspended:
                    case ScheduledMobilityRequestStatus.Closed:
                        return true;
                }
                return false;
            case ScheduledMobilityRequestStatus.Suspended:
                switch (next)
                {
                    case ScheduledMobilityRequestStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledMobilityRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled mobility request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scheduled mobility request"} {Code}";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public ScheduledMobilityRequest Copy()
    {
        return new ScheduledMobilityRequest(Code, Name, EmployeeId, Approved, Active, Archived, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
