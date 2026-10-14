namespace Acme.Workforce.Orgdesign.Position.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a position bundle is in its life cycle.</summary>
public enum PositionBundleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One position bundle, with what the planners need to act on it.</summary>
public sealed class PositionBundle(string code, string name, string department, string unit, string summary, string employeeId, double amount, PositionBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Department { get; set; } = department;
    public string Unit { get; set; } = unit;
    public string Summary { get; set; } = summary;
    public string EmployeeId { get; set; } = employeeId;
    public double Amount { get; set; } = amount;
    public PositionBundleStatus Status { get; set; } = status;

    /// <summary>Starts a position bundle in status idle.</summary>
    public static PositionBundle NewPositionBundle(string code, string name)
    {
        return new PositionBundle(code, name, "", "", "", "", 0.0, PositionBundleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PositionBundleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PositionBundleStatus next)
    {
        switch (Status)
        {
            case PositionBundleStatus.Idle:
                return next == PositionBundleStatus.Draining || next == PositionBundleStatus.Stopped;
            case PositionBundleStatus.Warming:
                return next == PositionBundleStatus.Draining || next == PositionBundleStatus.Stopped;
            case PositionBundleStatus.Ready:
                switch (next)
                {
                    case PositionBundleStatus.Draining:
                        return true;
                }
                return false;
            case PositionBundleStatus.Draining:
                return next == PositionBundleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(PositionBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"position bundle {Code} cannot move from {Status} to {next}");
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

    public PositionBundle Copy()
    {
        return new PositionBundle(Code, Name, Department, Unit, Summary, EmployeeId, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
