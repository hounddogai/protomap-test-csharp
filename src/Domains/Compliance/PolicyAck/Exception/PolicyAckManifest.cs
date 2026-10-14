namespace Acme.Workforce.Compliance.PolicyAck.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy ack manifest is in its life cycle.</summary>
public enum PolicyAckManifestStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A policy ack manifest as the policy ack team keeps it.</summary>
public sealed class PolicyAckManifest(string code, string label, int windowDays, string locale, string title, int tenureMonths, bool approved, string currency, PolicyAckManifestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int WindowDays { get; set; } = windowDays;
    public string Locale { get; set; } = locale;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public bool Approved { get; set; } = approved;
    public string Currency { get; set; } = currency;
    public PolicyAckManifestStatus Status { get; set; } = status;

    /// <summary>Starts a policy ack manifest in status planned.</summary>
    public static PolicyAckManifest NewPolicyAckManifest(string code, string label)
    {
        return new PolicyAckManifest(code, label, 0, "", "", 0, false, "", PolicyAckManifestStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyAckManifestStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyAckManifestStatus next)
    {
        switch (Status)
        {
            case PolicyAckManifestStatus.Planned:
                switch (next)
                {
                    case PolicyAckManifestStatus.Scheduled:
                        return true;
                }
                return false;
            case PolicyAckManifestStatus.Scheduled:
                return next == PolicyAckManifestStatus.InProgress || next == PolicyAckManifestStatus.Cancelled;
            case PolicyAckManifestStatus.InProgress:
                switch (next)
                {
                    case PolicyAckManifestStatus.Completed:
                    case PolicyAckManifestStatus.Cancelled:
                        return true;
                }
                return false;
            case PolicyAckManifestStatus.Completed:
                return next == PolicyAckManifestStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(PolicyAckManifestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy ack manifest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveExposure()
    {
        return Math.Round((double)WindowDays * (double)TenureMonths, 2);
    }

    public PolicyAckManifest Copy()
    {
        return new PolicyAckManifest(
            Code,
            Label,
            WindowDays,
            Locale,
            Title,
            TenureMonths,
            Approved,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
