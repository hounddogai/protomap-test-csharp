namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a batched span ledger is in its life cycle.</summary>
public enum BatchedSpanLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One batched span ledger, with what the planners need to act on it.</summary>
public sealed class BatchedSpanLedger(string code, string title, Dictionary<string, string> attributes, double budget, int quantity, bool visible, string source, string locale, string segment, string unit, BatchedSpanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double Budget { get; set; } = budget;
    public int Quantity { get; set; } = quantity;
    public bool Visible { get; set; } = visible;
    public string Source { get; set; } = source;
    public string Locale { get; set; } = locale;
    public string Segment { get; set; } = segment;
    public string Unit { get; set; } = unit;
    public BatchedSpanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a batched span ledger in status open.</summary>
    public static BatchedSpanLedger NewBatchedSpanLedger(string code, string title)
    {
        return new BatchedSpanLedger(
            code,
            title,
            new Dictionary<string, string>(),
            0.0,
            0,
            false,
            "",
            "",
            "",
            "",
            BatchedSpanLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BatchedSpanLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BatchedSpanLedgerStatus next)
    {
        switch (Status)
        {
            case BatchedSpanLedgerStatus.Open:
                return next == BatchedSpanLedgerStatus.Reopened;
            case BatchedSpanLedgerStatus.InReview:
                switch (next)
                {
                    case BatchedSpanLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedSpanLedgerStatus.Resolved:
                switch (next)
                {
                    case BatchedSpanLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case BatchedSpanLedgerStatus.Reopened:
                switch (next)
                {
                    case BatchedSpanLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BatchedSpanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"batched span ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double NetVolume()
    {
        return Math.Round((double)Quantity * Budget, 2);
    }

    public BatchedSpanLedger Copy()
    {
        return new BatchedSpanLedger(
            Code,
            Title,
            Attributes,
            Budget,
            Quantity,
            Visible,
            Source,
            Locale,
            Segment,
            Unit,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Source);
    }
}
