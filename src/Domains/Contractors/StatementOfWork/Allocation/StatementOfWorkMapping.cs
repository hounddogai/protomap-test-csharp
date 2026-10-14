namespace Acme.Workforce.Contractors.StatementOfWork.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a statement of work mapping is in its life cycle.</summary>
public enum StatementOfWorkMappingStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a statement of work mapping and its current status.</summary>
public sealed class StatementOfWorkMapping(string code, string name, double amount, string employeeId, string location, bool active, StatementOfWorkMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Amount { get; set; } = amount;
    public string EmployeeId { get; set; } = employeeId;
    public string Location { get; set; } = location;
    public bool Active { get; set; } = active;
    public StatementOfWorkMappingStatus Status { get; set; } = status;

    /// <summary>Starts a statement of work mapping in status requested.</summary>
    public static StatementOfWorkMapping NewStatementOfWorkMapping(string code, string name)
    {
        return new StatementOfWorkMapping(code, name, 0.0, "", "", false, StatementOfWorkMappingStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StatementOfWorkMappingStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StatementOfWorkMappingStatus next)
    {
        switch (Status)
        {
            case StatementOfWorkMappingStatus.Requested:
                return next == StatementOfWorkMappingStatus.Granted || next == StatementOfWorkMappingStatus.Revoked;
            case StatementOfWorkMappingStatus.Granted:
                return next == StatementOfWorkMappingStatus.Revoked || next == StatementOfWorkMappingStatus.Expired;
            case StatementOfWorkMappingStatus.Revoked:
                return next == StatementOfWorkMappingStatus.Expired;
        }
        return false;
    }

    public void MoveTo(StatementOfWorkMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"statement of work mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double MeasuredValue()
    {
        return Amount;
    }

    public StatementOfWorkMapping Copy()
    {
        return new StatementOfWorkMapping(Code, Name, Amount, EmployeeId, Location, Active, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
