namespace Acme.Workforce.Workplace.AccessBadge.Notice;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary access badge summary is in its life cycle.</summary>
public enum PrimaryAccessBadgeSummaryStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A primary access badge summary. Its code is unique within a tenant.</summary>
public sealed class PrimaryAccessBadgeSummary(string code, string name, double weight, string locale, double margin, double cost, string currency, string category, bool pinned, PrimaryAccessBadgeSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Weight { get; set; } = weight;
    public string Locale { get; set; } = locale;
    public double Margin { get; set; } = margin;
    public double Cost { get; set; } = cost;
    public string Currency { get; set; } = currency;
    public string Category { get; set; } = category;
    public bool Pinned { get; set; } = pinned;
    public PrimaryAccessBadgeSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a primary access badge summary in status open.</summary>
    public static PrimaryAccessBadgeSummary NewPrimaryAccessBadgeSummary(string code, string name)
    {
        return new PrimaryAccessBadgeSummary(
            code,
            name,
            0.0,
            "",
            0.0,
            0.0,
            "",
            "",
            false,
            PrimaryAccessBadgeSummaryStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimaryAccessBadgeSummaryStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimaryAccessBadgeSummaryStatus next)
    {
        switch (Status)
        {
            case PrimaryAccessBadgeSummaryStatus.Open:
                switch (next)
                {
                    case PrimaryAccessBadgeSummaryStatus.InReview:
                    case PrimaryAccessBadgeSummaryStatus.Reopened:
                        return true;
                }
                return false;
            case PrimaryAccessBadgeSummaryStatus.InReview:
                return next == PrimaryAccessBadgeSummaryStatus.Resolved || next == PrimaryAccessBadgeSummaryStatus.Archived;
            case PrimaryAccessBadgeSummaryStatus.Resolved:
                return next == PrimaryAccessBadgeSummaryStatus.Reopened || next == PrimaryAccessBadgeSummaryStatus.Archived;
            case PrimaryAccessBadgeSummaryStatus.Reopened:
                switch (next)
                {
                    case PrimaryAccessBadgeSummaryStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimaryAccessBadgeSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary access badge summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"primary access badge summary"} {Code}";
    }

    public double AdjustedValue()
    {
        return Math.Round(Weight + Margin, 2);
    }

    public PrimaryAccessBadgeSummary Copy()
    {
        return new PrimaryAccessBadgeSummary(
            Code,
            Name,
            Weight,
            Locale,
            Margin,
            Cost,
            Currency,
            Category,
            Pinned,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
