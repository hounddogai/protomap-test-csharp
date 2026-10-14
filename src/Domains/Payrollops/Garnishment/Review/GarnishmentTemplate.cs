namespace Acme.Workforce.Payrollops.Garnishment.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment template is in its life cycle.</summary>
public enum GarnishmentTemplateStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a garnishment template and its current status.</summary>
public sealed class GarnishmentTemplate(string code, string title, int priority, double score, string label, string externalRef, double discount, string badge, GarnishmentTemplateStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public int Priority { get; set; } = priority;
    public double Score { get; set; } = score;
    public string Label { get; set; } = label;
    public string ExternalRef { get; set; } = externalRef;
    public double Discount { get; set; } = discount;
    public string Badge { get; set; } = badge;
    public GarnishmentTemplateStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment template in status draft.</summary>
    public static GarnishmentTemplate NewGarnishmentTemplate(string code, string title)
    {
        return new GarnishmentTemplate(code, title, 0, 0.0, "", "", 0.0, "", GarnishmentTemplateStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentTemplateStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentTemplateStatus next)
    {
        switch (Status)
        {
            case GarnishmentTemplateStatus.Draft:
                switch (next)
                {
                    case GarnishmentTemplateStatus.Pending:
                    case GarnishmentTemplateStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentTemplateStatus.Pending:
                return next == GarnishmentTemplateStatus.Suspended;
            case GarnishmentTemplateStatus.Active:
                switch (next)
                {
                    case GarnishmentTemplateStatus.Suspended:
                    case GarnishmentTemplateStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentTemplateStatus.Suspended:
                switch (next)
                {
                    case GarnishmentTemplateStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GarnishmentTemplateStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment template {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Title}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Priority - Discount, 2);
    }

    public GarnishmentTemplate Copy()
    {
        return new GarnishmentTemplate(Code, Title, Priority, Score, Label, ExternalRef, Discount, Badge, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
