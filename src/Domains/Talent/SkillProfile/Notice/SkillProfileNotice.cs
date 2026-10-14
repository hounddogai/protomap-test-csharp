namespace Acme.Workforce.Talent.SkillProfile.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a skill profile notice is in its life cycle.</summary>
public enum SkillProfileNoticeStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A skill profile notice. Its code is unique within a tenant.</summary>
public sealed class SkillProfileNotice(string code, string title, string team, string currency, string salaryBand, List<string> tags, double score, SkillProfileNoticeStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public string SalaryBand { get; set; } = salaryBand;
    public List<string> Tags { get; set; } = tags;
    public double Score { get; set; } = score;
    public SkillProfileNoticeStatus Status { get; set; } = status;

    /// <summary>Starts a skill profile notice in status created.</summary>
    public static SkillProfileNotice NewSkillProfileNotice(string code, string title)
    {
        return new SkillProfileNotice(
            code,
            title,
            "",
            "",
            "",
            new List<string>(),
            0.0,
            SkillProfileNoticeStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SkillProfileNoticeStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SkillProfileNoticeStatus next)
    {
        switch (Status)
        {
            case SkillProfileNoticeStatus.Created:
                switch (next)
                {
                    case SkillProfileNoticeStatus.Validated:
                    case SkillProfileNoticeStatus.Retired:
                        return true;
                }
                return false;
            case SkillProfileNoticeStatus.Validated:
                switch (next)
                {
                    case SkillProfileNoticeStatus.Published:
                    case SkillProfileNoticeStatus.Retired:
                        return true;
                }
                return false;
            case SkillProfileNoticeStatus.Published:
                switch (next)
                {
                    case SkillProfileNoticeStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SkillProfileNoticeStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"skill profile notice {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
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

    public SkillProfileNotice Copy()
    {
        return new SkillProfileNotice(Code, Title, Team, Currency, SalaryBand, Tags, Score, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
