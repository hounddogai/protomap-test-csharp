namespace Acme.Workforce.Compliance.PolicyAck.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack template is in its life cycle.</summary>
public enum PolicyAckTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a policy ack template and its current status.</summary>
public sealed class PolicyAckTemplate(string code, string name, bool pinned, int endsAt, int retries, int version, double fte, int attempts, string salaryBand, string employeeId, double rate, PolicyAckTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Pinned { get; set; } = pinned;
    public int EndsAt { get; set; } = endsAt;
    public int Retries { get; set; } = retries;
    public int Version { get; set; } = version;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public string SalaryBand { get; set; } = salaryBand;
    public string EmployeeId { get; set; } = employeeId;
    public double Rate { get; set; } = rate;
    public PolicyAckTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack template in status planned.</summary>
    public static PolicyAckTemplate NewPolicyAckTemplate(string code, string name)
    {
        return new PolicyAckTemplate(
            code,
            name,
            false,
            0,
            0,
            0,
            0.0,
            0,
            "",
            "",
            0.0,
            PolicyAckTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckTemplateStatus next)
    {
        switch (Status)
        {
            case PolicyAckTemplateStatus.Planned:
                return next == PolicyAckTemplateStatus.Completed;
            case PolicyAckTemplateStatus.Scheduled:
                switch (next)
                {
                    case PolicyAckTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckTemplateStatus.InProgress:
                switch (next)
                {
                    case PolicyAckTemplateStatus.Completed:
                    case PolicyAckTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckTemplateStatus.Completed:
                return next == PolicyAckTemplateStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PolicyAckTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Version += 1;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetExposure()
    {
        return Math.Round(Rate * (double)Attempts, 2);
    }

    public PolicyAckTemplate Copy()
    {
        return new PolicyAckTemplate(
            Code,
            Name,
            Pinned,
            EndsAt,
            Retries,
            Version,
            Fte,
            Attempts,
            SalaryBand,
            EmployeeId,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
