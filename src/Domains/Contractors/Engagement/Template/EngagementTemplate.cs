namespace Acme.Workforce.Contractors.Engagement.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a engagement template is in its life cycle.</summary>
public enum EngagementTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One engagement template, with what the planners need to act on it.</summary>
public sealed class EngagementTemplate(string code, string name, string phone, string summary, int attempts, string department, bool visible, int graceDays, int retries, string label, EngagementTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Phone { get; set; } = phone;
    public string Summary { get; set; } = summary;
    public int Attempts { get; set; } = attempts;
    public string Department { get; set; } = department;
    public bool Visible { get; set; } = visible;
    public int GraceDays { get; set; } = graceDays;
    public int Retries { get; set; } = retries;
    public string Label { get; set; } = label;
    public EngagementTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a engagement template in status draft.</summary>
    public static EngagementTemplate NewEngagementTemplate(string code, string name)
    {
        return new EngagementTemplate(code, name, "", "", 0, "", false, 0, 0, "", EngagementTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EngagementTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EngagementTemplateStatus next)
    {
        switch (Status)
        {
            case EngagementTemplateStatus.Draft:
                switch (next)
                {
                    case EngagementTemplateStatus.Suspended:
                    case EngagementTemplateStatus.Closed:
                        return true;
                }
                return false;
            case EngagementTemplateStatus.Pending:
                switch (next)
                {
                    case EngagementTemplateStatus.Active:
                        return true;
                }
                return false;
            case EngagementTemplateStatus.Active:
                return next == EngagementTemplateStatus.Suspended || next == EngagementTemplateStatus.Closed;
            case EngagementTemplateStatus.Suspended:
                return next == EngagementTemplateStatus.Closed;
        }
        return false;
    }

    public void MoveTo(EngagementTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"engagement template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"engagement template"} {Code}";
    }

    public double WeightedVolume()
    {
        return Math.Round((double)Attempts * (double)GraceDays, 2);
    }

    public EngagementTemplate Copy()
    {
        return new EngagementTemplate(
            Code,
            Name,
            Phone,
            Summary,
            Attempts,
            Department,
            Visible,
            GraceDays,
            Retries,
            Label,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
