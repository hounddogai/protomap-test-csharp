namespace Acme.Workforce.Talent.Mobility.Variance;

using Acme.Workforce.Kit.Text;

/// <summary>Where a mobility draft is in its life cycle.</summary>
public enum MobilityDraftStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One mobility draft, with what the planners need to act on it.</summary>
public sealed class MobilityDraft(string code, string name, string unit, int retries, string grade, string ownerId, Dictionary<string, string> attributes, string region, string externalRef, bool visible, MobilityDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Unit { get; set; } = unit;
    public int Retries { get; set; } = retries;
    public string Grade { get; set; } = grade;
    public string OwnerId { get; set; } = ownerId;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Region { get; set; } = region;
    public string ExternalRef { get; set; } = externalRef;
    public bool Visible { get; set; } = visible;
    public MobilityDraftStatus Status { get; set; } = status;

    /// <summary>Starts a mobility draft in status created.</summary>
    public static MobilityDraft NewMobilityDraft(string code, string name)
    {
        return new MobilityDraft(
            code,
            name,
            "",
            0,
            "",
            "",
            new Dictionary<string, string>(),
            "",
            "",
            false,
            MobilityDraftStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case MobilityDraftStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(MobilityDraftStatus next)
    {
        switch (Status)
        {
            case MobilityDraftStatus.Created:
                switch (next)
                {
                    case MobilityDraftStatus.Validated:
                    case MobilityDraftStatus.Retired:
                        return true;
                }
                return false;
            case MobilityDraftStatus.Validated:
                return next == MobilityDraftStatus.Published || next == MobilityDraftStatus.Retired;
            case MobilityDraftStatus.Published:
                return next == MobilityDraftStatus.Retired;
        }
        return false;
    }

    public void MoveTo(MobilityDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"mobility draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"mobility draft"} {Code}";
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
        return (double)Retries;
    }

    public MobilityDraft Copy()
    {
        return new MobilityDraft(
            Code,
            Name,
            Unit,
            Retries,
            Grade,
            OwnerId,
            Attributes,
            Region,
            ExternalRef,
            Visible,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
