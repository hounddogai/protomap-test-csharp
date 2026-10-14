namespace Acme.Workforce.Compliance.Certification.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a certification rollup is in its life cycle.</summary>
public enum CertificationRollupStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A certification rollup as the certification team keeps it.</summary>
public sealed class CertificationRollup(string code, string label, string costCenter, string location, int endsAt, string region, int startsAt, string segment, string channel, bool pinned, Dictionary<string, string> attributes, CertificationRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string CostCenter { get; set; } = costCenter;
    public string Location { get; set; } = location;
    public int EndsAt { get; set; } = endsAt;
    public string Region { get; set; } = region;
    public int StartsAt { get; set; } = startsAt;
    public string Segment { get; set; } = segment;
    public string Channel { get; set; } = channel;
    public bool Pinned { get; set; } = pinned;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public CertificationRollupStatus Status { get; set; } = status;

    /// <summary>Starts a certification rollup in status proposed.</summary>
    public static CertificationRollup NewCertificationRollup(string code, string label)
    {
        return new CertificationRollup(
            code,
            label,
            "",
            "",
            0,
            "",
            0,
            "",
            "",
            false,
            new Dictionary<string, string>(),
            CertificationRollupStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CertificationRollupStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CertificationRollupStatus next)
    {
        switch (Status)
        {
            case CertificationRollupStatus.Proposed:
                return next == CertificationRollupStatus.Rejected;
            case CertificationRollupStatus.Approved:
                switch (next)
                {
                    case CertificationRollupStatus.Withdrawn:
                        return true;
                }
                return false;
            case CertificationRollupStatus.Rejected:
                switch (next)
                {
                    case CertificationRollupStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CertificationRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"certification rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"certification rollup"} {Code}";
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

    public double EffectiveExposure()
    {
        return Math.Round((double)StartsAt + (double)EndsAt, 2);
    }

    public CertificationRollup Copy()
    {
        return new CertificationRollup(
            Code,
            Label,
            CostCenter,
            Location,
            EndsAt,
            Region,
            StartsAt,
            Segment,
            Channel,
            Pinned,
            Attributes,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
