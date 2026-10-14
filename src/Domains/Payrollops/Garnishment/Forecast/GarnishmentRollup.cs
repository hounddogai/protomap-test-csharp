namespace Acme.Workforce.Payrollops.Garnishment.Forecast;

using Acme.Workforce.Kit.Text;

/// <summary>Where a garnishment rollup is in its life cycle.</summary>
public enum GarnishmentRollupStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a garnishment rollup and its current status.</summary>
public sealed class GarnishmentRollup(string code, string label, string notes, string title, double rate, int version, int batchSize, string reviewer, string summary, int capacity, GarnishmentRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Notes { get; set; } = notes;
    public string Title { get; set; } = title;
    public double Rate { get; set; } = rate;
    public int Version { get; set; } = version;
    public int BatchSize { get; set; } = batchSize;
    public string Reviewer { get; set; } = reviewer;
    public string Summary { get; set; } = summary;
    public int Capacity { get; set; } = capacity;
    public GarnishmentRollupStatus Status { get; set; } = status;

    /// <summary>Starts a garnishment rollup in status open.</summary>
    public static GarnishmentRollup NewGarnishmentRollup(string code, string label)
    {
        return new GarnishmentRollup(code, label, "", "", 0.0, 0, 0, "", "", 0, GarnishmentRollupStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GarnishmentRollupStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GarnishmentRollupStatus next)
    {
        switch (Status)
        {
            case GarnishmentRollupStatus.Open:
                switch (next)
                {
                    case GarnishmentRollupStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentRollupStatus.InReview:
                switch (next)
                {
                    case GarnishmentRollupStatus.Reopened:
                        return true;
                }
                return false;
            case GarnishmentRollupStatus.Resolved:
                return next == GarnishmentRollupStatus.Reopened;
            case GarnishmentRollupStatus.Reopened:
                return next == GarnishmentRollupStatus.Archived;
        }
        return false;
    }

    public void MoveTo(GarnishmentRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"garnishment rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        Version += 1;
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetVolume()
    {
        return Math.Round((double)BatchSize - (double)Capacity, 2);
    }

    public GarnishmentRollup Copy()
    {
        return new GarnishmentRollup(
            Code,
            Label,
            Notes,
            Title,
            Rate,
            Version,
            BatchSize,
            Reviewer,
            Summary,
            Capacity,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
