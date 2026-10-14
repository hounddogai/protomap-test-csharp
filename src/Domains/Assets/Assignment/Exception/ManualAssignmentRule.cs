namespace Acme.Workforce.Assets.Assignment.Exception;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual assignment rule is in its life cycle.</summary>
public enum ManualAssignmentRuleStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A manual assignment rule as the assignment team keeps it.</summary>
public sealed class ManualAssignmentRule(string code, string label, string channel, List<string> tags, bool visible, string location, string phone, double score, ManualAssignmentRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public List<string> Tags { get; set; } = tags;
    public bool Visible { get; set; } = visible;
    public string Location { get; set; } = location;
    public string Phone { get; set; } = phone;
    public double Score { get; set; } = score;
    public ManualAssignmentRuleStatus Status { get; set; } = status;

    /// <summary>Starts a manual assignment rule in status draft.</summary>
    public static ManualAssignmentRule NewManualAssignmentRule(string code, string label)
    {
        return new ManualAssignmentRule(
            code,
            label,
            "",
            new List<string>(),
            false,
            "",
            "",
            0.0,
            ManualAssignmentRuleStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualAssignmentRuleStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualAssignmentRuleStatus next)
    {
        switch (Status)
        {
            case ManualAssignmentRuleStatus.Draft:
                switch (next)
                {
                    case ManualAssignmentRuleStatus.Pending:
                    case ManualAssignmentRuleStatus.Active:
                        return true;
                }
                return false;
            case ManualAssignmentRuleStatus.Pending:
                return next == ManualAssignmentRuleStatus.Active || next == ManualAssignmentRuleStatus.Closed;
            case ManualAssignmentRuleStatus.Active:
                return next == ManualAssignmentRuleStatus.Suspended || next == ManualAssignmentRuleStatus.Closed;
            case ManualAssignmentRuleStatus.Suspended:
                return next == ManualAssignmentRuleStatus.Closed;
        }
        return false;
    }

    public void MoveTo(ManualAssignmentRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual assignment rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
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
        return Score;
    }

    public ManualAssignmentRule Copy()
    {
        return new ManualAssignmentRule(Code, Label, Channel, Tags, Visible, Location, Phone, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
