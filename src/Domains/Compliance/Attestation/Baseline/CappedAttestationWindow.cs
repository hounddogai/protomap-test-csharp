namespace Acme.Workforce.Compliance.Attestation.Baseline;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped attestation window is in its life cycle.</summary>
public enum CappedAttestationWindowStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>One capped attestation window, with what the planners need to act on it.</summary>
public sealed class CappedAttestationWindow(string code, string name, Dictionary<string, string> attributes, string managerId, string locale, bool archived, string externalRef, double rate, CappedAttestationWindowStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string ManagerId { get; set; } = managerId;
    public string Locale { get; set; } = locale;
    public bool Archived { get; set; } = archived;
    public string ExternalRef { get; set; } = externalRef;
    public double Rate { get; set; } = rate;
    public CappedAttestationWindowStatus Status { get; set; } = status;

    /// <summary>Starts a capped attestation window in status planned.</summary>
    public static CappedAttestationWindow NewCappedAttestationWindow(string code, string name)
    {
        return new CappedAttestationWindow(
            code,
            name,
            new Dictionary<string, string>(),
            "",
            "",
            false,
            "",
            0.0,
            CappedAttestationWindowStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAttestationWindowStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAttestationWindowStatus next)
    {
        switch (Status)
        {
            case CappedAttestationWindowStatus.Planned:
                switch (next)
                {
                    case CappedAttestationWindowStatus.Completed:
                        return true;
                }
                return false;
            case CappedAttestationWindowStatus.Scheduled:
                switch (next)
                {
                    case CappedAttestationWindowStatus.InProgress:
                    case CappedAttestationWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedAttestationWindowStatus.InProgress:
                switch (next)
                {
                    case CappedAttestationWindowStatus.Completed:
                    case CappedAttestationWindowStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedAttestationWindowStatus.Completed:
                switch (next)
                {
                    case CappedAttestationWindowStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAttestationWindowStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped attestation window {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
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

    public double MeasuredValue()
    {
        return Rate;
    }

    public CappedAttestationWindow Copy()
    {
        return new CappedAttestationWindow(
            Code,
            Name,
            Attributes,
            ManagerId,
            Locale,
            Archived,
            ExternalRef,
            Rate,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
