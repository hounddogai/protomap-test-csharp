namespace Acme.Workforce.Compensation.Adjustment.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary adjustment preference is in its life cycle.</summary>
public enum PrimaryAdjustmentPreferenceStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a primary adjustment preference and its current status.</summary>
public sealed class PrimaryAdjustmentPreference(string code, string label, string skill, string location, List<string> tags, string badge, double amount, PrimaryAdjustmentPreferenceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Skill { get; set; } = skill;
    public string Location { get; set; } = location;
    public List<string> Tags { get; set; } = tags;
    public string Badge { get; set; } = badge;
    public double Amount { get; set; } = amount;
    public PrimaryAdjustmentPreferenceStatus Status { get; set; } = status;

    /// <summary>Starts a primary adjustment preference in status requested.</summary>
    public static PrimaryAdjustmentPreference NewPrimaryAdjustmentPreference(string code, string label)
    {
        return new PrimaryAdjustmentPreference(
            code,
            label,
            "",
            "",
            new List<string>(),
            "",
            0.0,
            PrimaryAdjustmentPreferenceStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAdjustmentPreferenceStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAdjustmentPreferenceStatus next)
    {
        switch (Status)
        {
            case PrimaryAdjustmentPreferenceStatus.Requested:
                switch (next)
                {
                    case PrimaryAdjustmentPreferenceStatus.Granted:
                        return true;
                }
                return false;
            case PrimaryAdjustmentPreferenceStatus.Granted:
                switch (next)
                {
                    case PrimaryAdjustmentPreferenceStatus.Revoked:
                        return true;
                }
                return false;
            case PrimaryAdjustmentPreferenceStatus.Revoked:
                switch (next)
                {
                    case PrimaryAdjustmentPreferenceStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAdjustmentPreferenceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary adjustment preference {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
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

    public double MeasuredValue()
    {
        return Amount;
    }

    public PrimaryAdjustmentPreference Copy()
    {
        return new PrimaryAdjustmentPreference(Code, Label, Skill, Location, Tags, Badge, Amount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
