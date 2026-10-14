namespace Acme.Workforce.Workplace.Desk.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a desk template is in its life cycle.</summary>
public enum DeskTemplateStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One desk template, with what the planners need to act on it.</summary>
public sealed class DeskTemplate(string code, string label, string country, int retries, string description, double margin, string region, string currency, string name, double ratio, int tenureMonths, DeskTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Country { get; set; } = country;
    public int Retries { get; set; } = retries;
    public string Description { get; set; } = description;
    public double Margin { get; set; } = margin;
    public string Region { get; set; } = region;
    public string Currency { get; set; } = currency;
    public string Name { get; set; } = name;
    public double Ratio { get; set; } = ratio;
    public int TenureMonths { get; set; } = tenureMonths;
    public DeskTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a desk template in status open.</summary>
    public static DeskTemplate NewDeskTemplate(string code, string label)
    {
        return new DeskTemplate(code, label, "", 0, "", 0.0, "", "", "", 0.0, 0, DeskTemplateStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case DeskTemplateStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(DeskTemplateStatus next)
    {
        switch (Status)
        {
            case DeskTemplateStatus.Open:
                return next == DeskTemplateStatus.Archived;
            case DeskTemplateStatus.InReview:
                switch (next)
                {
                    case DeskTemplateStatus.Reopened:
                    case DeskTemplateStatus.Archived:
                        return true;
                }
                return false;
            case DeskTemplateStatus.Resolved:
                return next == DeskTemplateStatus.Reopened || next == DeskTemplateStatus.Archived;
            case DeskTemplateStatus.Reopened:
                return next == DeskTemplateStatus.Archived;
        }
        return false;
    }

    public void MoveTo(DeskTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"desk template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)TenureMonths - Ratio, 2);
    }

    public DeskTemplate Copy()
    {
        return new DeskTemplate(
            Code,
            Label,
            Country,
            Retries,
            Description,
            Margin,
            Region,
            Currency,
            Name,
            Ratio,
            TenureMonths,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
