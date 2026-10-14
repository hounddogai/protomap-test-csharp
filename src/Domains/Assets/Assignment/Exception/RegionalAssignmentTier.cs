namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional assignment tier is in its life cycle.</summary>
public enum RegionalAssignmentTierStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a regional assignment tier and its current status.</summary>
public sealed class RegionalAssignmentTier(string code, string name, bool visible, string grade, string skill, int limit, string notes, string title, RegionalAssignmentTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public bool Visible { get; set; } = visible;
    public string Grade { get; set; } = grade;
    public string Skill { get; set; } = skill;
    public int Limit { get; set; } = limit;
    public string Notes { get; set; } = notes;
    public string Title { get; set; } = title;
    public RegionalAssignmentTierStatus Status { get; set; } = status;

    /// <summary>Starts a regional assignment tier in status created.</summary>
    public static RegionalAssignmentTier NewRegionalAssignmentTier(string code, string name)
    {
        return new RegionalAssignmentTier(
            code,
            name,
            false,
            "",
            "",
            0,
            "",
            "",
            RegionalAssignmentTierStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalAssignmentTierStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalAssignmentTierStatus next)
    {
        switch (Status)
        {
            case RegionalAssignmentTierStatus.Created:
                switch (next)
                {
                    case RegionalAssignmentTierStatus.Validated:
                    case RegionalAssignmentTierStatus.Published:
                        return true;
                }
                return false;
            case RegionalAssignmentTierStatus.Validated:
                switch (next)
                {
                    case RegionalAssignmentTierStatus.Published:
                    case RegionalAssignmentTierStatus.Retired:
                        return true;
                }
                return false;
            case RegionalAssignmentTierStatus.Published:
                return next == RegionalAssignmentTierStatus.Retired;
        }
        return false;
    }

    public void MoveTo(RegionalAssignmentTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional assignment tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Name}";
    }

    public double MeasuredValue()
    {
        return (double)Limit;
    }

    public RegionalAssignmentTier Copy()
    {
        return new RegionalAssignmentTier(Code, Name, Visible, Grade, Skill, Limit, Notes, Title, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
