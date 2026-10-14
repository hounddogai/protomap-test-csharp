namespace Acme.Workforce.Compliance.PolicyAck.AckProfile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack variance is in its life cycle.</summary>
public enum PolicyAckVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A policy ack variance as the policy ack team keeps it.</summary>
public sealed class PolicyAckVariance(string code, string title, bool archived, double minimum, string employeeId, List<string> tags, int graceDays, string currency, int attempts, PolicyAckVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public double Minimum { get; set; } = minimum;
    public string EmployeeId { get; set; } = employeeId;
    public List<string> Tags { get; set; } = tags;
    public int GraceDays { get; set; } = graceDays;
    public string Currency { get; set; } = currency;
    public int Attempts { get; set; } = attempts;
    public PolicyAckVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack variance in status created.</summary>
    public static PolicyAckVariance NewPolicyAckVariance(string code, string title)
    {
        return new PolicyAckVariance(
            code,
            title,
            false,
            0.0,
            "",
            new List<string>(),
            0,
            "",
            0,
            PolicyAckVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckVarianceStatus next)
    {
        switch (Status)
        {
            case PolicyAckVarianceStatus.Created:
                switch (next)
                {
                    case PolicyAckVarianceStatus.Published:
                        return true;
                }
                return false;
            case PolicyAckVarianceStatus.Validated:
                return next == PolicyAckVarianceStatus.Published;
            case PolicyAckVarianceStatus.Published:
                return next == PolicyAckVarianceStatus.Retired;
        }
        return false;
    }

    public void MoveTo(PolicyAckVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double ProjectedLoad()
    {
        return Math.Round(Minimum - (double)GraceDays, 2);
    }

    public PolicyAckVariance Copy()
    {
        return new PolicyAckVariance(
            Code,
            Title,
            Archived,
            Minimum,
            EmployeeId,
            Tags,
            GraceDays,
            Currency,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
