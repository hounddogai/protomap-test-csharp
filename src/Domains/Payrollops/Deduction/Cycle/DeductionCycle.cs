namespace Acme.Workforce.Payrollops.Deduction.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a deduction cycle is in its life cycle.</summary>
public enum DeductionCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a deduction cycle and its current status.</summary>
public sealed class DeductionCycle(string code, string name, int endsAt, string department, string region, int batchSize, string segment, string managerId, string label, string skill, DeductionCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int EndsAt { get; set; } = endsAt;
    public string Department { get; set; } = department;
    public string Region { get; set; } = region;
    public int BatchSize { get; set; } = batchSize;
    public string Segment { get; set; } = segment;
    public string ManagerId { get; set; } = managerId;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public DeductionCycleStatus Status { get; set; } = status;

    /// <summary>Starts a deduction cycle in status requested.</summary>
    public static DeductionCycle NewDeductionCycle(string code, string name)
    {
        return new DeductionCycle(code, name, 0, "", "", 0, "", "", "", "", DeductionCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeductionCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeductionCycleStatus next)
    {
        switch (Status)
        {
            case DeductionCycleStatus.Requested:
                return next == DeductionCycleStatus.Granted || next == DeductionCycleStatus.Revoked;
            case DeductionCycleStatus.Granted:
                switch (next)
                {
                    case DeductionCycleStatus.Expired:
                        return true;
                }
                return false;
            case DeductionCycleStatus.Revoked:
                return next == DeductionCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(DeductionCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"deduction cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Name})";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)EndsAt - (double)BatchSize, 2);
    }

    public DeductionCycle Copy()
    {
        return new DeductionCycle(
            Code,
            Name,
            EndsAt,
            Department,
            Region,
            BatchSize,
            Segment,
            ManagerId,
            Label,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
