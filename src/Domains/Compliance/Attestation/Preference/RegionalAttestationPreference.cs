namespace Acme.Workforce.Compliance.Attestation.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional attestation preference is in its life cycle.</summary>
public enum RegionalAttestationPreferenceStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A regional attestation preference. Its code is unique within a tenant.</summary>
public sealed class RegionalAttestationPreference(string code, string title, string timezone, int expiresAt, double budget, Dictionary<string, string> attributes, int sequence, double score, RegionalAttestationPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Timezone { get; set; } = timezone;
    public int ExpiresAt { get; set; } = expiresAt;
    public double Budget { get; set; } = budget;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Sequence { get; set; } = sequence;
    public double Score { get; set; } = score;
    public RegionalAttestationPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a regional attestation preference in status planned.</summary>
    public static RegionalAttestationPreference NewRegionalAttestationPreference(string code, string title)
    {
        return new RegionalAttestationPreference(
            code,
            title,
            "",
            0,
            0.0,
            new Dictionary<string, string>(),
            0,
            0.0,
            RegionalAttestationPreferenceStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAttestationPreferenceStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAttestationPreferenceStatus next)
    {
        switch (Status)
        {
            case RegionalAttestationPreferenceStatus.Planned:
                switch (next)
                {
                    case RegionalAttestationPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalAttestationPreferenceStatus.Scheduled:
                switch (next)
                {
                    case RegionalAttestationPreferenceStatus.Completed:
                        return true;
                }
                return false;
            case RegionalAttestationPreferenceStatus.InProgress:
                switch (next)
                {
                    case RegionalAttestationPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
            case RegionalAttestationPreferenceStatus.Completed:
                switch (next)
                {
                    case RegionalAttestationPreferenceStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalAttestationPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional attestation preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        ExpiresAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public double WeightedLoad()
    {
        return Math.Round((double)ExpiresAt - Budget, 2);
    }

    public RegionalAttestationPreference Copy()
    {
        return new RegionalAttestationPreference(
            Code,
            Title,
            Timezone,
            ExpiresAt,
            Budget,
            Attributes,
            Sequence,
            Score,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
