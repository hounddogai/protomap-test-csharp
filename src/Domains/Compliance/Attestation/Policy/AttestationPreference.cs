namespace Acme.Workforce.Compliance.Attestation.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation preference is in its life cycle.</summary>
public enum AttestationPreferenceStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a attestation preference and its current status.</summary>
public sealed class AttestationPreference(string code, string label, string currency, List<string> tags, int quantity, int expiresAt, AttestationPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Currency { get; set; } = currency;
    public List<string> Tags { get; set; } = tags;
    public int Quantity { get; set; } = quantity;
    public int ExpiresAt { get; set; } = expiresAt;
    public AttestationPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a attestation preference in status open.</summary>
    public static AttestationPreference NewAttestationPreference(string code, string label)
    {
        return new AttestationPreference(
            code,
            label,
            "",
            new List<string>(),
            0,
            0,
            AttestationPreferenceStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationPreferenceStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationPreferenceStatus next)
    {
        switch (Status)
        {
            case AttestationPreferenceStatus.Open:
                switch (next)
                {
                    case AttestationPreferenceStatus.InReview:
                    case AttestationPreferenceStatus.Resolved:
                        return true;
                }
                return false;
            case AttestationPreferenceStatus.InReview:
                switch (next)
                {
                    case AttestationPreferenceStatus.Reopened:
                    case AttestationPreferenceStatus.Archived:
                        return true;
                }
                return false;
            case AttestationPreferenceStatus.Resolved:
                return next == AttestationPreferenceStatus.Reopened || next == AttestationPreferenceStatus.Archived;
            case AttestationPreferenceStatus.Reopened:
                switch (next)
                {
                    case AttestationPreferenceStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"attestation preference"} {Code}";
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

    public double AdjustedExposure()
    {
        return Math.Round((double)Quantity * (double)ExpiresAt, 2);
    }

    public AttestationPreference Copy()
    {
        return new AttestationPreference(Code, Label, Currency, Tags, Quantity, ExpiresAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
