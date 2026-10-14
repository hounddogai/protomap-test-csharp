namespace Acme.Workforce.Compliance.PolicyAck.Profile;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary policy ack mapping is in its life cycle.</summary>
public enum PrimaryPolicyAckMappingStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One primary policy ack mapping, with what the planners need to act on it.</summary>
public sealed class PrimaryPolicyAckMapping(string code, string label, string skill, double rating, int windowDays, string location, int sequence, string locale, string currency, double threshold, int retries, PrimaryPolicyAckMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public double Rating { get; set; } = rating;
    public int WindowDays { get; set; } = windowDays;
    public string Location { get; set; } = location;
    public int Sequence { get; set; } = sequence;
    public string Locale { get; set; } = locale;
    public string Currency { get; set; } = currency;
    public double Threshold { get; set; } = threshold;
    public int Retries { get; set; } = retries;
    public PrimaryPolicyAckMappingStatus Status { get; set; } = status;

    /// <summary>Starts a primary policy ack mapping in status created.</summary>
    public static PrimaryPolicyAckMapping NewPrimaryPolicyAckMapping(string code, string label)
    {
        return new PrimaryPolicyAckMapping(
            code,
            label,
            "",
            0.0,
            0,
            "",
            0,
            "",
            "",
            0.0,
            0,
            PrimaryPolicyAckMappingStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryPolicyAckMappingStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryPolicyAckMappingStatus next)
    {
        switch (Status)
        {
            case PrimaryPolicyAckMappingStatus.Created:
                return next == PrimaryPolicyAckMappingStatus.Validated || next == PrimaryPolicyAckMappingStatus.Published;
            case PrimaryPolicyAckMappingStatus.Validated:
                switch (next)
                {
                    case PrimaryPolicyAckMappingStatus.Published:
                    case PrimaryPolicyAckMappingStatus.Retired:
                        return true;
                }
                return false;
            case PrimaryPolicyAckMappingStatus.Published:
                switch (next)
                {
                    case PrimaryPolicyAckMappingStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryPolicyAckMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary policy ack mapping {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedValue()
    {
        return Math.Round((double)WindowDays - Threshold, 2);
    }

    public PrimaryPolicyAckMapping Copy()
    {
        return new PrimaryPolicyAckMapping(
            Code,
            Label,
            Skill,
            Rating,
            WindowDays,
            Location,
            Sequence,
            Locale,
            Currency,
            Threshold,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
