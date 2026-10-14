namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack cycle is in its life cycle.</summary>
public enum PolicyAckCycleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A policy ack cycle. Its code is unique within a tenant.</summary>
public sealed class PolicyAckCycle(string code, string name, string region, string salaryBand, string unit, string jobCode, string grade, string locale, double score, PolicyAckCycleStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Region { get; set; } = region;
    public string SalaryBand { get; set; } = salaryBand;
    public string Unit { get; set; } = unit;
    public string JobCode { get; set; } = jobCode;
    public string Grade { get; set; } = grade;
    public string Locale { get; set; } = locale;
    public double Score { get; set; } = score;
    public PolicyAckCycleStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack cycle in status created.</summary>
    public static PolicyAckCycle NewPolicyAckCycle(string code, string name)
    {
        return new PolicyAckCycle(code, name, "", "", "", "", "", "", 0.0, PolicyAckCycleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckCycleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckCycleStatus next)
    {
        switch (Status)
        {
            case PolicyAckCycleStatus.Created:
                switch (next)
                {
                    case PolicyAckCycleStatus.Published:
                    case PolicyAckCycleStatus.Retired:
                        return true;
                }
                return false;
            case PolicyAckCycleStatus.Validated:
                return next == PolicyAckCycleStatus.Published || next == PolicyAckCycleStatus.Retired;
            case PolicyAckCycleStatus.Published:
                switch (next)
                {
                    case PolicyAckCycleStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckCycleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack cycle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"policy ack cycle"} {Code}";
    }

    public double MeasuredValue()
    {
        return Score;
    }

    public PolicyAckCycle Copy()
    {
        return new PolicyAckCycle(Code, Name, Region, SalaryBand, Unit, JobCode, Grade, Locale, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
