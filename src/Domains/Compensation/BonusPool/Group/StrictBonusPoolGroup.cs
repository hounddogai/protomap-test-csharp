namespace Acme.Workforce.Compensation.BonusPool.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict bonus pool group is in its life cycle.</summary>
public enum StrictBonusPoolGroupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One strict bonus pool group, with what the planners need to act on it.</summary>
public sealed class StrictBonusPoolGroup(string code, string title, int tenureMonths, string ownerId, string employeeId, string channel, string managerId, StrictBonusPoolGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public string OwnerId { get; set; } = ownerId;
    public string EmployeeId { get; set; } = employeeId;
    public string Channel { get; set; } = channel;
    public string ManagerId { get; set; } = managerId;
    public StrictBonusPoolGroupStatus Status { get; set; } = status;

    /// <summary>Starts a strict bonus pool group in status created.</summary>
    public static StrictBonusPoolGroup NewStrictBonusPoolGroup(string code, string title)
    {
        return new StrictBonusPoolGroup(code, title, 0, "", "", "", "", StrictBonusPoolGroupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictBonusPoolGroupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictBonusPoolGroupStatus next)
    {
        switch (Status)
        {
            case StrictBonusPoolGroupStatus.Created:
                return next == StrictBonusPoolGroupStatus.Validated || next == StrictBonusPoolGroupStatus.Retired;
            case StrictBonusPoolGroupStatus.Validated:
                switch (next)
                {
                    case StrictBonusPoolGroupStatus.Retired:
                        return true;
                }
                return false;
            case StrictBonusPoolGroupStatus.Published:
                switch (next)
                {
                    case StrictBonusPoolGroupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(StrictBonusPoolGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict bonus pool group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double MeasuredValue()
    {
        return (double)TenureMonths;
    }

    public StrictBonusPoolGroup Copy()
    {
        return new StrictBonusPoolGroup(Code, Title, TenureMonths, OwnerId, EmployeeId, Channel, ManagerId, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
