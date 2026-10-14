namespace Acme.Workforce.Talent.Nomination.Request;

using Acme.Workforce.Kit.Text;

/// <summary>Where a fallback nomination bundle is in its life cycle.</summary>
public enum FallbackNominationBundleStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A fallback nomination bundle as the nomination team keeps it.</summary>
public sealed class FallbackNominationBundle(string code, string title, string grade, int updatedAt, Dictionary<string, string> attributes, string costCenter, int sequence, string source, string externalRef, double ratio, string currency, FallbackNominationBundleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Grade { get; set; } = grade;
    public int UpdatedAt { get; set; } = updatedAt;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string CostCenter { get; set; } = costCenter;
    public int Sequence { get; set; } = sequence;
    public string Source { get; set; } = source;
    public string ExternalRef { get; set; } = externalRef;
    public double Ratio { get; set; } = ratio;
    public string Currency { get; set; } = currency;
    public FallbackNominationBundleStatus Status { get; set; } = status;

    /// <summary>Starts a fallback nomination bundle in status proposed.</summary>
    public static FallbackNominationBundle NewFallbackNominationBundle(string code, string title)
    {
        return new FallbackNominationBundle(
            code,
            title,
            "",
            0,
            new Dictionary<string, string>(),
            "",
            0,
            "",
            "",
            0.0,
            "",
            FallbackNominationBundleStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case FallbackNominationBundleStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(FallbackNominationBundleStatus next)
    {
        switch (Status)
        {
            case FallbackNominationBundleStatus.Proposed:
                switch (next)
                {
                    case FallbackNominationBundleStatus.Approved:
                    case FallbackNominationBundleStatus.Withdrawn:
                        return true;
                }
                return false;
            case FallbackNominationBundleStatus.Approved:
                switch (next)
                {
                    case FallbackNominationBundleStatus.Rejected:
                        return true;
                }
                return false;
            case FallbackNominationBundleStatus.Rejected:
                return next == FallbackNominationBundleStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(FallbackNominationBundleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"fallback nomination bundle {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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

    public double AdjustedVolume()
    {
        return Math.Round(Ratio + (double)Sequence, 2);
    }

    public FallbackNominationBundle Copy()
    {
        return new FallbackNominationBundle(
            Code,
            Title,
            Grade,
            UpdatedAt,
            Attributes,
            CostCenter,
            Sequence,
            Source,
            ExternalRef,
            Ratio,
            Currency,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
