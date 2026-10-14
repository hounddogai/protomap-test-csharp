namespace Acme.Workforce.Compliance.PolicyAck.Quota;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack quota is in its life cycle.</summary>
public enum PolicyAckQuotaStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One policy ack quota, with what the planners need to act on it.</summary>
public sealed class PolicyAckQuota(string code, string title, string summary, string category, double rate, PolicyAckQuotaStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Summary { get; set; } = summary;
    public string Category { get; set; } = category;
    public double Rate { get; set; } = rate;
    public PolicyAckQuotaStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack quota in status created.</summary>
    public static PolicyAckQuota NewPolicyAckQuota(string code, string title)
    {
        return new PolicyAckQuota(code, title, "", "", 0.0, PolicyAckQuotaStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckQuotaStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckQuotaStatus next)
    {
        switch (Status)
        {
            case PolicyAckQuotaStatus.Created:
                switch (next)
                {
                    case PolicyAckQuotaStatus.Validated:
                        return true;
                }
                return false;
            case PolicyAckQuotaStatus.Validated:
                return next == PolicyAckQuotaStatus.Published;
            case PolicyAckQuotaStatus.Published:
                switch (next)
                {
                    case PolicyAckQuotaStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAckQuotaStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack quota {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double MeasuredValue()
    {
        return Rate;
    }

    public PolicyAckQuota Copy()
    {
        return new PolicyAckQuota(Code, Title, Summary, Category, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
