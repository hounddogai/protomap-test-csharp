namespace Acme.Workforce.Compliance.Attestation.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a attestation digest is in its life cycle.</summary>
public enum AttestationDigestStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>The record of a attestation digest and its current status.</summary>
public sealed class AttestationDigest(string code, string title, int tenureMonths, Dictionary<string, string> attributes, int version, AttestationDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int TenureMonths { get; set; } = tenureMonths;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public int Version { get; set; } = version;
    public AttestationDigestStatus Status { get; set; } = status;

    /// <summary>Starts a attestation digest in status new.</summary>
    public static AttestationDigest NewAttestationDigest(string code, string title)
    {
        return new AttestationDigest(
            code,
            title,
            0,
            new Dictionary<string, string>(),
            0,
            AttestationDigestStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AttestationDigestStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AttestationDigestStatus next)
    {
        switch (Status)
        {
            case AttestationDigestStatus.New:
                return next == AttestationDigestStatus.Running || next == AttestationDigestStatus.Failed;
            case AttestationDigestStatus.Queued:
                return next == AttestationDigestStatus.Running || next == AttestationDigestStatus.Done;
            case AttestationDigestStatus.Running:
                return next == AttestationDigestStatus.Failed;
            case AttestationDigestStatus.Done:
                switch (next)
                {
                    case AttestationDigestStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AttestationDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"attestation digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double WeightedVolume()
    {
        return Math.Round((double)Version + (double)TenureMonths, 2);
    }

    public AttestationDigest Copy()
    {
        return new AttestationDigest(Code, Title, TenureMonths, Attributes, Version, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
