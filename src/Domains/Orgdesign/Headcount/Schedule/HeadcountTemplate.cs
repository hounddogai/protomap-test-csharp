namespace Acme.Workforce.Orgdesign.Headcount.Schedule;

using Acme.Workforce.Kit.Text;

/// <summary>Where a headcount template is in its life cycle.</summary>
public enum HeadcountTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A headcount template. Its code is unique within a tenant.</summary>
public sealed class HeadcountTemplate(string code, string title, int endsAt, string team, string grade, string department, int tenureMonths, HeadcountTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int EndsAt { get; set; } = endsAt;
    public string Team { get; set; } = team;
    public string Grade { get; set; } = grade;
    public string Department { get; set; } = department;
    public int TenureMonths { get; set; } = tenureMonths;
    public HeadcountTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a headcount template in status draft.</summary>
    public static HeadcountTemplate NewHeadcountTemplate(string code, string title)
    {
        return new HeadcountTemplate(code, title, 0, "", "", "", 0, HeadcountTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case HeadcountTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(HeadcountTemplateStatus next)
    {
        switch (Status)
        {
            case HeadcountTemplateStatus.Draft:
                return next == HeadcountTemplateStatus.Pending || next == HeadcountTemplateStatus.Active;
            case HeadcountTemplateStatus.Pending:
                switch (next)
                {
                    case HeadcountTemplateStatus.Active:
                    case HeadcountTemplateStatus.Closed:
                        return true;
                }
                return false;
            case HeadcountTemplateStatus.Active:
                switch (next)
                {
                    case HeadcountTemplateStatus.Suspended:
                    case HeadcountTemplateStatus.Closed:
                        return true;
                }
                return false;
            case HeadcountTemplateStatus.Suspended:
                switch (next)
                {
                    case HeadcountTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(HeadcountTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"headcount template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)EndsAt - (double)TenureMonths, 2);
    }

    public HeadcountTemplate Copy()
    {
        return new HeadcountTemplate(Code, Title, EndsAt, Team, Grade, Department, TenureMonths, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
