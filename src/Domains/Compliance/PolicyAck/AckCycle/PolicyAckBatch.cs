namespace Acme.Workforce.Compliance.PolicyAck.AckCycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack batch is in its life cycle.</summary>
public enum PolicyAckBatchStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a policy ack batch and its current status.</summary>
public sealed class PolicyAckBatch(string code, string name, int retries, string employeeId, string contactEmail, int createdAt, double budget, double ratio, int revision, bool visible, int endsAt, PolicyAckBatchStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public string EmployeeId { get; set; } = employeeId;
    public string ContactEmail { get; set; } = contactEmail;
    public int CreatedAt { get; set; } = createdAt;
    public double Budget { get; set; } = budget;
    public double Ratio { get; set; } = ratio;
    public int Revision { get; set; } = revision;
    public bool Visible { get; set; } = visible;
    public int EndsAt { get; set; } = endsAt;
    public PolicyAckBatchStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack batch in status created.</summary>
    public static PolicyAckBatch NewPolicyAckBatch(string code, string name)
    {
        return new PolicyAckBatch(code, name, 0, "", "", 0, 0.0, 0.0, 0, false, 0, PolicyAckBatchStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckBatchStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckBatchStatus next)
    {
        switch (Status)
        {
            case PolicyAckBatchStatus.Created:
                return next == PolicyAckBatchStatus.Validated || next == PolicyAckBatchStatus.Published;
            case PolicyAckBatchStatus.Validated:
                switch (next)
                {
                    case PolicyAckBatchStatus.Published:
                    case PolicyAckBatchStatus.Retired:
                        return true;
                }
                return false;
            case PolicyAckBatchStatus.Published:
                return next == PolicyAckBatchStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PolicyAckBatchStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack batch {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{"policy ack batch"} {Code}";
    }

    public double ProjectedTotal()
    {
        return Math.Round((double)Retries + (double)CreatedAt, 2);
    }

    public PolicyAckBatch Copy()
    {
        return new PolicyAckBatch(
            Code,
            Name,
            Retries,
            EmployeeId,
            ContactEmail,
            CreatedAt,
            Budget,
            Ratio,
            Revision,
            Visible,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
