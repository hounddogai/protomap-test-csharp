namespace Acme.Workforce.Talent.SuccessionPlan.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan rollup is in its life cycle.</summary>
public enum SuccessionPlanRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One succession plan rollup, with what the planners need to act on it.</summary>
public sealed class SuccessionPlanRollup(string code, string label, string team, List<string> tags, string timezone, string salaryBand, string location, int quantity, SuccessionPlanRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Team { get; set; } = team;
    public List<string> Tags { get; set; } = tags;
    public string Timezone { get; set; } = timezone;
    public string SalaryBand { get; set; } = salaryBand;
    public string Location { get; set; } = location;
    public int Quantity { get; set; } = quantity;
    public SuccessionPlanRollupStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan rollup in status open.</summary>
    public static SuccessionPlanRollup NewSuccessionPlanRollup(string code, string label)
    {
        return new SuccessionPlanRollup(
            code,
            label,
            "",
            new List<string>(),
            "",
            "",
            "",
            0,
            SuccessionPlanRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanRollupStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanRollupStatus.Open:
                switch (next)
                {
                    case SuccessionPlanRollupStatus.InReview:
                        return true;
                }
                return false;
            case SuccessionPlanRollupStatus.InReview:
                switch (next)
                {
                    case SuccessionPlanRollupStatus.Resolved:
                    case SuccessionPlanRollupStatus.Reopened:
                        return true;
                }
                return false;
            case SuccessionPlanRollupStatus.Resolved:
                return next == SuccessionPlanRollupStatus.Reopened || next == SuccessionPlanRollupStatus.Archived;
            case SuccessionPlanRollupStatus.Reopened:
                switch (next)
                {
                    case SuccessionPlanRollupStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan rollup {Code} cannot move from {Status} to {next}");
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
        return (double)Quantity;
    }

    public SuccessionPlanRollup Copy()
    {
        return new SuccessionPlanRollup(Code, Label, Team, Tags, Timezone, SalaryBand, Location, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
