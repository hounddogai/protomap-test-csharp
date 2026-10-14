namespace Acme.Workforce.Engagement.Sentiment.Rollup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a primary sentiment rollup is in its life cycle.</summary>
public enum PrimarySentimentRollupStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a primary sentiment rollup and its current status.</summary>
public sealed class PrimarySentimentRollup(string code, string label, int quantity, bool approved, string team, int batchSize, string phone, string country, string currency, int retries, PrimarySentimentRollupStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public string Team { get; set; } = team;
    public int BatchSize { get; set; } = batchSize;
    public string Phone { get; set; } = phone;
    public string Country { get; set; } = country;
    public string Currency { get; set; } = currency;
    public int Retries { get; set; } = retries;
    public PrimarySentimentRollupStatus Status { get; set; } = status;

    /// <summary>Starts a primary sentiment rollup in status created.</summary>
    public static PrimarySentimentRollup NewPrimarySentimentRollup(string code, string label)
    {
        return new PrimarySentimentRollup(
            code,
            label,
            0,
            false,
            "",
            0,
            "",
            "",
            "",
            0,
            PrimarySentimentRollupStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PrimarySentimentRollupStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PrimarySentimentRollupStatus next)
    {
        switch (Status)
        {
            case PrimarySentimentRollupStatus.Created:
                switch (next)
                {
                    case PrimarySentimentRollupStatus.Validated:
                    case PrimarySentimentRollupStatus.Published:
                        return true;
                }
                return false;
            case PrimarySentimentRollupStatus.Validated:
                return next == PrimarySentimentRollupStatus.Retired;
            case PrimarySentimentRollupStatus.Published:
                switch (next)
                {
                    case PrimarySentimentRollupStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PrimarySentimentRollupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"primary sentiment rollup {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"primary sentiment rollup"} {Code}";
    }

    public double AdjustedLoad()
    {
        return Math.Round((double)Retries * (double)Quantity, 2);
    }

    public PrimarySentimentRollup Copy()
    {
        return new PrimarySentimentRollup(
            Code,
            Label,
            Quantity,
            Approved,
            Team,
            BatchSize,
            Phone,
            Country,
            Currency,
            Retries,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Team);
    }
}
