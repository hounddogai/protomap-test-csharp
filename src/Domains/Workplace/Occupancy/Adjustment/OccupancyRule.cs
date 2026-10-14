namespace Acme.Workforce.Workplace.Occupancy.Adjustment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a occupancy rule is in its life cycle.</summary>
public enum OccupancyRuleStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One occupancy rule, with what the planners need to act on it.</summary>
public sealed class OccupancyRule(string code, string title, string department, int revision, string phone, List<string> tags, string grade, double rate, double discount, string salaryBand, string skill, OccupancyRuleStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Department { get; set; } = department;
    public int Revision { get; set; } = revision;
    public string Phone { get; set; } = phone;
    public List<string> Tags { get; set; } = tags;
    public string Grade { get; set; } = grade;
    public double Rate { get; set; } = rate;
    public double Discount { get; set; } = discount;
    public string SalaryBand { get; set; } = salaryBand;
    public string Skill { get; set; } = skill;
    public OccupancyRuleStatus Status { get; set; } = status;

    /// <summary>Starts a occupancy rule in status created.</summary>
    public static OccupancyRule NewOccupancyRule(string code, string title)
    {
        return new OccupancyRule(
            code,
            title,
            "",
            0,
            "",
            new List<string>(),
            "",
            0.0,
            0.0,
            "",
            "",
            OccupancyRuleStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case OccupancyRuleStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(OccupancyRuleStatus next)
    {
        switch (Status)
        {
            case OccupancyRuleStatus.Created:
                switch (next)
                {
                    case OccupancyRuleStatus.Published:
                    case OccupancyRuleStatus.Retired:
                        return true;
                }
                return false;
            case OccupancyRuleStatus.Validated:
                return next == OccupancyRuleStatus.Published || next == OccupancyRuleStatus.Retired;
            case OccupancyRuleStatus.Published:
                return next == OccupancyRuleStatus.Retired;
        }
        return false;
    }

    public void MoveTo(OccupancyRuleStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"occupancy rule {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double ProjectedVolume()
    {
        return Math.Round((double)Revision - Rate, 2);
    }

    public OccupancyRule Copy()
    {
        return new OccupancyRule(
            Code,
            Title,
            Department,
            Revision,
            Phone,
            Tags,
            Grade,
            Rate,
            Discount,
            SalaryBand,
            Skill,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
