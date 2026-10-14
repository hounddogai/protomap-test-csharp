namespace Acme.Workforce.Payrollops.Garnishment.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment budget is in its life cycle.</summary>
public enum GarnishmentBudgetStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>The record of a garnishment budget and its current status.</summary>
public sealed class GarnishmentBudget(string code, string title, string externalRef, string notes, string timezone, int batchSize, string jobCode, double rating, bool archived, double margin, GarnishmentBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string ExternalRef { get; set; } = externalRef;
    public string Notes { get; set; } = notes;
    public string Timezone { get; set; } = timezone;
    public int BatchSize { get; set; } = batchSize;
    public string JobCode { get; set; } = jobCode;
    public double Rating { get; set; } = rating;
    public bool Archived { get; set; } = archived;
    public double Margin { get; set; } = margin;
    public GarnishmentBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment budget in status draft.</summary>
    public static GarnishmentBudget NewGarnishmentBudget(string code, string title)
    {
        return new GarnishmentBudget(code, title, "", "", "", 0, "", 0.0, false, 0.0, GarnishmentBudgetStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentBudgetStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentBudgetStatus next)
    {
        switch (Status)
        {
            case GarnishmentBudgetStatus.Draft:
                switch (next)
                {
                    case GarnishmentBudgetStatus.Active:
                    case GarnishmentBudgetStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentBudgetStatus.Pending:
                switch (next)
                {
                    case GarnishmentBudgetStatus.Active:
                    case GarnishmentBudgetStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentBudgetStatus.Active:
                switch (next)
                {
                    case GarnishmentBudgetStatus.Suspended:
                    case GarnishmentBudgetStatus.Closed:
                        return true;
                }
                return false;
            case GarnishmentBudgetStatus.Suspended:
                return next == GarnishmentBudgetStatus.Closed;
        }
        return false;
    }

    public void MoveTo(GarnishmentBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
    }

    public double ProjectedVolume()
    {
        return Math.Round(Margin * (double)BatchSize, 2);
    }

    public GarnishmentBudget Copy()
    {
        return new GarnishmentBudget(
            Code,
            Title,
            ExternalRef,
            Notes,
            Timezone,
            BatchSize,
            JobCode,
            Rating,
            Archived,
            Margin,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
