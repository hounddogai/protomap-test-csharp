namespace Acme.Workforce.Orgdesign.Reorg.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg cycle is in its life cycle.</summary>
public enum ReorgCycleStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One reorg cycle, with what the planners need to act on it.</summary>
public sealed class ReorgCycle(string code, string label, string employeeId, int retries, double discount, string department, string location, string summary, string description, string channel, string phone, ReorgCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string EmployeeId { get; set; } = employeeId;
    public int Retries { get; set; } = retries;
    public double Discount { get; set; } = discount;
    public string Department { get; set; } = department;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public string Description { get; set; } = description;
    public string Channel { get; set; } = channel;
    public string Phone { get; set; } = phone;
    public ReorgCycleStatus Status { get; set; } = status;

    /// <summary>Starts a reorg cycle in status idle.</summary>
    public static ReorgCycle NewReorgCycle(string code, string label)
    {
        return new ReorgCycle(code, label, "", 0, 0.0, "", "", "", "", "", "", ReorgCycleStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgCycleStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgCycleStatus next)
    {
        switch (Status)
        {
            case ReorgCycleStatus.Idle:
                switch (next)
                {
                    case ReorgCycleStatus.Warming:
                    case ReorgCycleStatus.Draining:
                        return true;
                }
                return false;
            case ReorgCycleStatus.Warming:
                return next == ReorgCycleStatus.Draining || next == ReorgCycleStatus.Stopped;
            case ReorgCycleStatus.Ready:
                return next == ReorgCycleStatus.Stopped;
            case ReorgCycleStatus.Draining:
                return next == ReorgCycleStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(ReorgCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedVolume()
    {
        return Math.Round(Discount + (double)Retries, 2);
    }

    public ReorgCycle Copy()
    {
        return new ReorgCycle(
            Code,
            Label,
            EmployeeId,
            Retries,
            Discount,
            Department,
            Location,
            Summary,
            Description,
            Channel,
            Phone,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
