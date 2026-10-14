namespace Acme.Workforce.Payrollops.TaxForm.Template;

using Acme.Workforce.Kit.Text;

/// <summary>Where a tax form template is in its life cycle.</summary>
public enum TaxFormTemplateStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A tax form template as the tax form team keeps it.</summary>
public sealed class TaxFormTemplate(string code, string label, string ownerId, bool visible, List<string> tags, double rating, string description, double threshold, double minimum, int sequence, string summary, TaxFormTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string OwnerId { get; set; } = ownerId;
    public bool Visible { get; set; } = visible;
    public List<string> Tags { get; set; } = tags;
    public double Rating { get; set; } = rating;
    public string Description { get; set; } = description;
    public double Threshold { get; set; } = threshold;
    public double Minimum { get; set; } = minimum;
    public int Sequence { get; set; } = sequence;
    public string Summary { get; set; } = summary;
    public TaxFormTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a tax form template in status created.</summary>
    public static TaxFormTemplate NewTaxFormTemplate(string code, string label)
    {
        return new TaxFormTemplate(
            code,
            label,
            "",
            false,
            new List<string>(),
            0.0,
            "",
            0.0,
            0.0,
            0,
            "",
            TaxFormTemplateStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case TaxFormTemplateStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(TaxFormTemplateStatus next)
    {
        switch (Status)
        {
            case TaxFormTemplateStatus.Created:
                switch (next)
                {
                    case TaxFormTemplateStatus.Published:
                        return true;
                }
                return false;
            case TaxFormTemplateStatus.Validated:
                switch (next)
                {
                    case TaxFormTemplateStatus.Published:
                    case TaxFormTemplateStatus.Retired:
                        return true;
                }
                return false;
            case TaxFormTemplateStatus.Published:
                return next == TaxFormTemplateStatus.Retired;
        }
        return false;
    }

    public void MoveTo(TaxFormTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"tax form template {Code} cannot move from {Status} to {next}");
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

    public double AdjustedValue()
    {
        return Math.Round(Threshold - Rating, 2);
    }

    public TaxFormTemplate Copy()
    {
        return new TaxFormTemplate(
            Code,
            Label,
            OwnerId,
            Visible,
            Tags,
            Rating,
            Description,
            Threshold,
            Minimum,
            Sequence,
            Summary,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
