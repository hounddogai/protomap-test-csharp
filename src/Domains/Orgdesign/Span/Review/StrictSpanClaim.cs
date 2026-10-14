namespace Acme.Workforce.Orgdesign.Span.Review;

using Acme.Workforce.Kit.Text;

/// <summary>Where a strict span claim is in its life cycle.</summary>
public enum StrictSpanClaimStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A strict span claim as the span team keeps it.</summary>
public sealed class StrictSpanClaim(string code, string name, int batchSize, int quantity, string title, double ratio, double rating, StrictSpanClaimStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int BatchSize { get; set; } = batchSize;
    public int Quantity { get; set; } = quantity;
    public string Title { get; set; } = title;
    public double Ratio { get; set; } = ratio;
    public double Rating { get; set; } = rating;
    public StrictSpanClaimStatus Status { get; set; } = status;

    /// <summary>Starts a strict span claim in status planned.</summary>
    public static StrictSpanClaim NewStrictSpanClaim(string code, string name)
    {
        return new StrictSpanClaim(code, name, 0, 0, "", 0.0, 0.0, StrictSpanClaimStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case StrictSpanClaimStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(StrictSpanClaimStatus next)
    {
        switch (Status)
        {
            case StrictSpanClaimStatus.Planned:
                switch (next)
                {
                    case StrictSpanClaimStatus.Scheduled:
                    case StrictSpanClaimStatus.Cancelled:
                        return true;
                }
                return false;
            case StrictSpanClaimStatus.Scheduled:
                switch (next)
                {
                    case StrictSpanClaimStatus.InProgress:
                        return true;
                }
                return false;
            case StrictSpanClaimStatus.InProgress:
                return next == StrictSpanClaimStatus.Cancelled;
            case StrictSpanClaimStatus.Completed:
                return next == StrictSpanClaimStatus.Cancelled;
        }
        return false;
    }

    public void MoveTo(StrictSpanClaimStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"strict span claim {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double WeightedTotal()
    {
        return Math.Round(Ratio * (double)Quantity, 2);
    }

    public StrictSpanClaim Copy()
    {
        return new StrictSpanClaim(Code, Name, BatchSize, Quantity, Title, Ratio, Rating, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
