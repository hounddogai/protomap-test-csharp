namespace Acme.Workforce.Talent.SkillProfile.ProfileGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a default skill profile group is in its life cycle.</summary>
public enum DefaultSkillProfileGroupStatus
{
    Requested,
    Granted,
    Revoked,
    Expired,
}

/// <summary>The record of a default skill profile group and its current status.</summary>
public sealed class DefaultSkillProfileGroup(string code, string label, string unit, string title, string contactEmail, int batchSize, DefaultSkillProfileGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Title { get; set; } = title;
    public string ContactEmail { get; set; } = contactEmail;
    public int BatchSize { get; set; } = batchSize;
    public DefaultSkillProfileGroupStatus Status { get; set; } = status;

    /// <summary>Starts a default skill profile group in status requested.</summary>
    public static DefaultSkillProfileGroup NewDefaultSkillProfileGroup(string code, string label)
    {
        return new DefaultSkillProfileGroup(code, label, "", "", "", 0, DefaultSkillProfileGroupStatus.Requested);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DefaultSkillProfileGroupStatus.Expired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DefaultSkillProfileGroupStatus next)
    {
        switch (Status)
        {
            case DefaultSkillProfileGroupStatus.Requested:
                switch (next)
                {
                    case DefaultSkillProfileGroupStatus.Granted:
                        return true;
                }
                return false;
            case DefaultSkillProfileGroupStatus.Granted:
                switch (next)
                {
                    case DefaultSkillProfileGroupStatus.Expired:
                        return true;
                }
                return false;
            case DefaultSkillProfileGroupStatus.Revoked:
                switch (next)
                {
                    case DefaultSkillProfileGroupStatus.Expired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(DefaultSkillProfileGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"default skill profile group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"default skill profile group"} {Code}";
    }

    public double MeasuredValue()
    {
        return (double)BatchSize;
    }

    public DefaultSkillProfileGroup Copy()
    {
        return new DefaultSkillProfileGroup(Code, Label, Unit, Title, ContactEmail, BatchSize, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
