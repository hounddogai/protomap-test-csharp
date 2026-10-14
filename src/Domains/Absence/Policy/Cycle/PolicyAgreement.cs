namespace Acme.Workforce.Absence.Policy.Cycle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy agreement is in its life cycle.</summary>
public enum PolicyAgreementStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A policy agreement. Its code is unique within a tenant.</summary>
public sealed class PolicyAgreement(string code, string label, string checksum, Dictionary<string, string> attributes, string contactEmail, double discount, double amount, string managerId, int limit, string summary, int updatedAt, PolicyAgreementStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Checksum { get; set; } = checksum;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ContactEmail { get; set; } = contactEmail;
    public double Discount { get; set; } = discount;
    public double Amount { get; set; } = amount;
    public string ManagerId { get; set; } = managerId;
    public int Limit { get; set; } = limit;
    public string Summary { get; set; } = summary;
    public int UpdatedAt { get; set; } = updatedAt;
    public PolicyAgreementStatus Status { get; set; } = status;

    /// <summary>Starts a policy agreement in status planned.</summary>
    public static PolicyAgreement NewPolicyAgreement(string code, string label)
    {
        return new PolicyAgreement(
            code,
            label,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            0.0,
            "",
            0,
            "",
            0,
            PolicyAgreementStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAgreementStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAgreementStatus next)
    {
        switch (Status)
        {
            case PolicyAgreementStatus.Planned:
                switch (next)
                {
                    case PolicyAgreementStatus.Scheduled:
                    case PolicyAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAgreementStatus.Scheduled:
                switch (next)
                {
                    case PolicyAgreementStatus.Completed:
                    case PolicyAgreementStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAgreementStatus.InProgress:
                switch (next)
                {
                    case PolicyAgreementStatus.Completed:
                        return true;
                }
                return false;
            case PolicyAgreementStatus.Completed:
                switch (next)
                {
                    case PolicyAgreementStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyAgreementStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy agreement {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedExposure()
    {
        return Math.Round(Amount * Discount, 2);
    }

    public PolicyAgreement Copy()
    {
        return new PolicyAgreement(
            Code,
            Label,
            Checksum,
            Attributes,
            ContactEmail,
            Discount,
            Amount,
            ManagerId,
            Limit,
            Summary,
            UpdatedAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
