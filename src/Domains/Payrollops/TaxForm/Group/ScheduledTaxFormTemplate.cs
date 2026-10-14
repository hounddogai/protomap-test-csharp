namespace Acme.Workforce.Payrollops.TaxForm.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled tax form template is in its life cycle.</summary>
public enum ScheduledTaxFormTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One scheduled tax form template, with what the planners need to act on it.</summary>
public sealed class ScheduledTaxFormTemplate(string code, string name, string externalRef, int endsAt, string skill, string reviewer, string badge, bool active, string grade, int tenureMonths, string region, ScheduledTaxFormTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string ExternalRef { get; set; } = externalRef;
    public int EndsAt { get; set; } = endsAt;
    public string Skill { get; set; } = skill;
    public string Reviewer { get; set; } = reviewer;
    public string Badge { get; set; } = badge;
    public bool Active { get; set; } = active;
    public string Grade { get; set; } = grade;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Region { get; set; } = region;
    public ScheduledTaxFormTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled tax form template in status created.</summary>
    public static ScheduledTaxFormTemplate NewScheduledTaxFormTemplate(string code, string name)
    {
        return new ScheduledTaxFormTemplate(
            code,
            name,
            "",
            0,
            "",
            "",
            "",
            false,
            "",
            0,
            "",
            ScheduledTaxFormTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledTaxFormTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledTaxFormTemplateStatus next)
    {
        switch (Status)
        {
            case ScheduledTaxFormTemplateStatus.Created:
                return next == ScheduledTaxFormTemplateStatus.Retired;
            case ScheduledTaxFormTemplateStatus.Validated:
                switch (next)
                {
                    case ScheduledTaxFormTemplateStatus.Published:
                    case ScheduledTaxFormTemplateStatus.Retired:
                        return true;
                }
                return false;
            case ScheduledTaxFormTemplateStatus.Published:
                switch (next)
                {
                    case ScheduledTaxFormTemplateStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledTaxFormTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled tax form template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"scheduled tax form template"} {Code}";
    }

    public double ProjectedVolume()
    {
        return Math.Round((double)TenureMonths - (double)EndsAt, 2);
    }

    public ScheduledTaxFormTemplate Copy()
    {
        return new ScheduledTaxFormTemplate(
            Code,
            Name,
            ExternalRef,
            EndsAt,
            Skill,
            Reviewer,
            Badge,
            Active,
            Grade,
            TenureMonths,
            Region,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
