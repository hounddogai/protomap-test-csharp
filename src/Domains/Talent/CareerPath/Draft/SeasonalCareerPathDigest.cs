namespace Acme.Workforce.Talent.CareerPath.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal career path digest is in its life cycle.</summary>
public enum SeasonalCareerPathDigestStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>A seasonal career path digest as the career path team keeps it.</summary>
public sealed class SeasonalCareerPathDigest(string code, string label, double discount, string phone, double hours, Dictionary<string, string> attributes, SeasonalCareerPathDigestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Phone { get; set; } = phone;
    public double Hours { get; set; } = hours;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public SeasonalCareerPathDigestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal career path digest in status requested.</summary>
    public static SeasonalCareerPathDigest NewSeasonalCareerPathDigest(string code, string label)
    {
        return new SeasonalCareerPathDigest(
            code,
            label,
            0.0,
            "",
            0.0,
            new Dictionary<string, string>(),
            SeasonalCareerPathDigestStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalCareerPathDigestStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalCareerPathDigestStatus next)
    {
        switch (Status)
        {
            case SeasonalCareerPathDigestStatus.Requested:
                switch (next)
                {
                    case SeasonalCareerPathDigestStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalCareerPathDigestStatus.Granted:
                switch (next)
                {
                    case SeasonalCareerPathDigestStatus.Revoked:
                    case SeasonalCareerPathDigestStatus.Expired:
                        return true;
                }
                return false;
            case SeasonalCareerPathDigestStatus.Revoked:
                return next == SeasonalCareerPathDigestStatus.Expired;
        }
        return false;
    }

    public void MoveTo(SeasonalCareerPathDigestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal career path digest {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double WeightedValue()
    {
        return Math.Round(Hours - Discount, 2);
    }

    public SeasonalCareerPathDigest Copy()
    {
        return new SeasonalCareerPathDigest(Code, Label, Discount, Phone, Hours, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
