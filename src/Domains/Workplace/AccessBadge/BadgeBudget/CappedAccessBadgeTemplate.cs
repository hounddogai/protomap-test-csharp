namespace Acme.Workforce.Workplace.AccessBadge.BadgeBudget;

using Acme.Workforce.Kit.Text;

/// <summary>Where a capped access badge template is in its life cycle.</summary>
public enum CappedAccessBadgeTemplateStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>The record of a capped access badge template and its current status.</summary>
public sealed class CappedAccessBadgeTemplate(string code, string title, double ceiling, double threshold, string summary, double floorValue, bool visible, Dictionary<string, string> attributes, double minimum, int quantity, string source, CappedAccessBadgeTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Ceiling { get; set; } = ceiling;
    public double Threshold { get; set; } = threshold;
    public string Summary { get; set; } = summary;
    public double FloorValue { get; set; } = floorValue;
    public bool Visible { get; set; } = visible;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Minimum { get; set; } = minimum;
    public int Quantity { get; set; } = quantity;
    public string Source { get; set; } = source;
    public CappedAccessBadgeTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a capped access badge template in status planned.</summary>
    public static CappedAccessBadgeTemplate NewCappedAccessBadgeTemplate(string code, string title)
    {
        return new CappedAccessBadgeTemplate(
            code,
            title,
            0.0,
            0.0,
            "",
            0.0,
            false,
            new Dictionary<string, string>(),
            0.0,
            0,
            "",
            CappedAccessBadgeTemplateStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CappedAccessBadgeTemplateStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CappedAccessBadgeTemplateStatus next)
    {
        switch (Status)
        {
            case CappedAccessBadgeTemplateStatus.Planned:
                switch (next)
                {
                    case CappedAccessBadgeTemplateStatus.InProgress:
                    case CappedAccessBadgeTemplateStatus.Completed:
                        return true;
                }
                return false;
            case CappedAccessBadgeTemplateStatus.Scheduled:
                return next == CappedAccessBadgeTemplateStatus.InProgress || next == CappedAccessBadgeTemplateStatus.Completed;
            case CappedAccessBadgeTemplateStatus.InProgress:
                switch (next)
                {
                    case CappedAccessBadgeTemplateStatus.Completed:
                    case CappedAccessBadgeTemplateStatus.Cancelled:
                        return true;
                }
                return false;
            case CappedAccessBadgeTemplateStatus.Completed:
                switch (next)
                {
                    case CappedAccessBadgeTemplateStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CappedAccessBadgeTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"capped access badge template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"capped access badge template"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double AdjustedTotal()
    {
        return Math.Round(Threshold - Minimum, 2);
    }

    public CappedAccessBadgeTemplate Copy()
    {
        return new CappedAccessBadgeTemplate(
            Code,
            Title,
            Ceiling,
            Threshold,
            Summary,
            FloorValue,
            Visible,
            Attributes,
            Minimum,
            Quantity,
            Source,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
