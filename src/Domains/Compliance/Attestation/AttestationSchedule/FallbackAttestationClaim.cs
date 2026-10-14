namespace Acme.Workforce.Compliance.Attestation.AttestationSchedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback attestation claim is in its life cycle.</summary>
public enum FallbackAttestationClaimStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One fallback attestation claim, with what the planners need to act on it.</summary>
public sealed class FallbackAttestationClaim(string code, string label, double floorValue, int retries, int quantity, List<string> tags, double amount, string externalRef, Dictionary<string, string> attributes, string costCenter, double maximum, FallbackAttestationClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double FloorValue { get; set; } = floorValue;
    public int Retries { get; set; } = retries;
    public int Quantity { get; set; } = quantity;
    public List<string> Tags { get; set; } = tags;
    public double Amount { get; set; } = amount;
    public string ExternalRef { get; set; } = externalRef;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string CostCenter { get; set; } = costCenter;
    public double Maximum { get; set; } = maximum;
    public FallbackAttestationClaimStatus Status { get; set; } = status;

    /// <summary>Starts a fallback attestation claim in status idle.</summary>
    public static FallbackAttestationClaim NewFallbackAttestationClaim(string code, string label)
    {
        return new FallbackAttestationClaim(
            code,
            label,
            0.0,
            0,
            0,
            new List<string>(),
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            0.0,
            FallbackAttestationClaimStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackAttestationClaimStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackAttestationClaimStatus next)
    {
        switch (Status)
        {
            case FallbackAttestationClaimStatus.Idle:
                return next == FallbackAttestationClaimStatus.Stopped;
            case FallbackAttestationClaimStatus.Warming:
                switch (next)
                {
                    case FallbackAttestationClaimStatus.Ready:
                    case FallbackAttestationClaimStatus.Stopped:
                        return true;
                }
                return false;
            case FallbackAttestationClaimStatus.Ready:
                return next == FallbackAttestationClaimStatus.Draining;
            case FallbackAttestationClaimStatus.Draining:
                switch (next)
                {
                    case FallbackAttestationClaimStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(FallbackAttestationClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback attestation claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"fallback attestation claim"} {Code}";
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

    public double NetLoad()
    {
        return Math.Round(Amount + (double)Quantity, 2);
    }

    public FallbackAttestationClaim Copy()
    {
        return new FallbackAttestationClaim(
            Code,
            Label,
            FloorValue,
            Retries,
            Quantity,
            Tags,
            Amount,
            ExternalRef,
            Attributes,
            CostCenter,
            Maximum,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
