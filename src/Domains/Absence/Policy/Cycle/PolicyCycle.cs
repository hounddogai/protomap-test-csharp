namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy cycle is in its life cycle.</summary>
public enum PolicyCycleStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>One policy cycle, with what the planners need to act on it.</summary>
public sealed class PolicyCycle(string code, string name, double rate, string title, string checksum, double budget, bool archived, PolicyCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Rate { get; set; } = rate;
    public string Title { get; set; } = title;
    public string Checksum { get; set; } = checksum;
    public double Budget { get; set; } = budget;
    public bool Archived { get; set; } = archived;
    public PolicyCycleStatus Status { get; set; } = status;

    /// <summary>Starts a policy cycle in status requested.</summary>
    public static PolicyCycle NewPolicyCycle(string code, string name)
    {
        return new PolicyCycle(code, name, 0.0, "", "", 0.0, false, PolicyCycleStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyCycleStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyCycleStatus next)
    {
        switch (Status)
        {
            case PolicyCycleStatus.Requested:
                switch (next)
                {
                    case PolicyCycleStatus.Granted:
                        return true;
                }
                return false;
            case PolicyCycleStatus.Granted:
                return next == PolicyCycleStatus.Revoked;
            case PolicyCycleStatus.Revoked:
                return next == PolicyCycleStatus.Expired;
        }
        return false;
    }

    public void MoveTo(PolicyCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Budget + Rate, 2);
    }

    public PolicyCycle Copy()
    {
        return new PolicyCycle(Code, Name, Rate, Title, Checksum, Budget, Archived, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
