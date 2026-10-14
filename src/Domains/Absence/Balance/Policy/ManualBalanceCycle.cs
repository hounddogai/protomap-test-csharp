namespace Acme.Workforce.Absence.Balance.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual balance cycle is in its life cycle.</summary>
public enum ManualBalanceCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A manual balance cycle. Its code is unique within a tenant.</summary>
public sealed class ManualBalanceCycle(string code, string name, int startsAt, int endsAt, string unit, string notes, string externalRef, string grade, double amount, string department, ManualBalanceCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int StartsAt { get; set; } = startsAt;
    public int EndsAt { get; set; } = endsAt;
    public string Unit { get; set; } = unit;
    public string Notes { get; set; } = notes;
    public string ExternalRef { get; set; } = externalRef;
    public string Grade { get; set; } = grade;
    public double Amount { get; set; } = amount;
    public string Department { get; set; } = department;
    public ManualBalanceCycleStatus Status { get; set; } = status;

    /// <summary>Starts a manual balance cycle in status created.</summary>
    public static ManualBalanceCycle NewManualBalanceCycle(string code, string name)
    {
        return new ManualBalanceCycle(code, name, 0, 0, "", "", "", "", 0.0, "", ManualBalanceCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualBalanceCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualBalanceCycleStatus next)
    {
        switch (Status)
        {
            case ManualBalanceCycleStatus.Created:
                return next == ManualBalanceCycleStatus.Published;
            case ManualBalanceCycleStatus.Validated:
                switch (next)
                {
                    case ManualBalanceCycleStatus.Published:
                    case ManualBalanceCycleStatus.Retired:
                        return true;
                }
                return false;
            case ManualBalanceCycleStatus.Published:
                return next == ManualBalanceCycleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(ManualBalanceCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual balance cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"manual balance cycle"} {Code}";
    }

    public double ProjectedLoad()
    {
        return Math.Round((double)StartsAt * Amount, 2);
    }

    public ManualBalanceCycle Copy()
    {
        return new ManualBalanceCycle(
            Code,
            Name,
            StartsAt,
            EndsAt,
            Unit,
            Notes,
            ExternalRef,
            Grade,
            Amount,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
