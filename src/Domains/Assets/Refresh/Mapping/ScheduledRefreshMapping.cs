namespace Acme.Workforce.Assets.Refresh.Mapping;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled refresh mapping is in its life cycle.</summary>
public enum ScheduledRefreshMappingStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a scheduled refresh mapping and its current status.</summary>
public sealed class ScheduledRefreshMapping(string code, string label, double minimum, double discount, string name, string salaryBand, string title, double rate, string team, string category, List<string> tags, ScheduledRefreshMappingStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Minimum { get; set; } = minimum;
    public double Discount { get; set; } = discount;
    public string Name { get; set; } = name;
    public string SalaryBand { get; set; } = salaryBand;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public string Category { get; set; } = category;
    public List<string> Tags { get; set; } = tags;
    public ScheduledRefreshMappingStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled refresh mapping in status proposed.</summary>
    public static ScheduledRefreshMapping NewScheduledRefreshMapping(string code, string label)
    {
        return new ScheduledRefreshMapping(
            code,
            label,
            0.0,
            0.0,
            "",
            "",
            "",
            0.0,
            "",
            "",
            new List<string>(),
            ScheduledRefreshMappingStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledRefreshMappingStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledRefreshMappingStatus next)
    {
        switch (Status)
        {
            case ScheduledRefreshMappingStatus.Proposed:
                switch (next)
                {
                    case ScheduledRefreshMappingStatus.Approved:
                        return true;
                }
                return false;
            case ScheduledRefreshMappingStatus.Approved:
                switch (next)
                {
                    case ScheduledRefreshMappingStatus.Rejected:
                    case ScheduledRefreshMappingStatus.Withdrawn:
                        return true;
                }
                return false;
            case ScheduledRefreshMappingStatus.Rejected:
                switch (next)
                {
                    case ScheduledRefreshMappingStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledRefreshMappingStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled refresh mapping {Code} cannot move from {Status} to {next}");
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

    public double WeightedVolume()
    {
        return Math.Round(Minimum + Rate, 2);
    }

    public ScheduledRefreshMapping Copy()
    {
        return new ScheduledRefreshMapping(
            Code,
            Label,
            Minimum,
            Discount,
            Name,
            SalaryBand,
            Title,
            Rate,
            Team,
            Category,
            Tags,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
