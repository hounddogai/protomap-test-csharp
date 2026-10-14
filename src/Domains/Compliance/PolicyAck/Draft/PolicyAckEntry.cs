namespace Acme.Workforce.Compliance.PolicyAck.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack entry is in its life cycle.</summary>
public enum PolicyAckEntryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A policy ack entry. Its code is unique within a tenant.</summary>
public sealed class PolicyAckEntry(string code, string label, string source, string badge, string unit, string country, int batchSize, string jobCode, string salaryBand, string costCenter, int attempts, PolicyAckEntryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Source { get; set; } = source;
    public string Badge { get; set; } = badge;
    public string Unit { get; set; } = unit;
    public string Country { get; set; } = country;
    public int BatchSize { get; set; } = batchSize;
    public string JobCode { get; set; } = jobCode;
    public string SalaryBand { get; set; } = salaryBand;
    public string CostCenter { get; set; } = costCenter;
    public int Attempts { get; set; } = attempts;
    public PolicyAckEntryStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack entry in status created.</summary>
    public static PolicyAckEntry NewPolicyAckEntry(string code, string label)
    {
        return new PolicyAckEntry(code, label, "", "", "", "", 0, "", "", "", 0, PolicyAckEntryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckEntryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckEntryStatus next)
    {
        switch (Status)
        {
            case PolicyAckEntryStatus.Created:
                switch (next)
                {
                    case PolicyAckEntryStatus.Validated:
                    case PolicyAckEntryStatus.Retired:
                        return true;
                }
                return false;
            case PolicyAckEntryStatus.Validated:
                return next == PolicyAckEntryStatus.Published;
            case PolicyAckEntryStatus.Published:
                switch (next)
                {
                    case PolicyAckEntryStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckEntryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack entry {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"policy ack entry"} {Code}";
    }

    public double WeightedValue()
    {
        return Math.Round((double)Attempts - (double)BatchSize, 2);
    }

    public PolicyAckEntry Copy()
    {
        return new PolicyAckEntry(
            Code,
            Label,
            Source,
            Badge,
            Unit,
            Country,
            BatchSize,
            JobCode,
            SalaryBand,
            CostCenter,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
