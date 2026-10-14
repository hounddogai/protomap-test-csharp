namespace Acme.Workforce.Orgdesign.Span.Digest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span ledger is in its life cycle.</summary>
public enum SpanLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A span ledger. Its code is unique within a tenant.</summary>
public sealed class SpanLedger(string code, string name, int capacity, string segment, int updatedAt, int leaveDays, string jobCode, int createdAt, string locale, bool approved, SpanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Capacity { get; set; } = capacity;
    public string Segment { get; set; } = segment;
    public int UpdatedAt { get; set; } = updatedAt;
    public int LeaveDays { get; set; } = leaveDays;
    public string JobCode { get; set; } = jobCode;
    public int CreatedAt { get; set; } = createdAt;
    public string Locale { get; set; } = locale;
    public bool Approved { get; set; } = approved;
    public SpanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a span ledger in status created.</summary>
    public static SpanLedger NewSpanLedger(string code, string name)
    {
        return new SpanLedger(code, name, 0, "", 0, 0, "", 0, "", false, SpanLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanLedgerStatus next)
    {
        switch (Status)
        {
            case SpanLedgerStatus.Created:
                switch (next)
                {
                    case SpanLedgerStatus.Validated:
                        return true;
                }
                return false;
            case SpanLedgerStatus.Validated:
                return next == SpanLedgerStatus.Published || next == SpanLedgerStatus.Retired;
            case SpanLedgerStatus.Published:
                return next == SpanLedgerStatus.Retired;
        }
        return false;
    }

    public void MoveTo(SpanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        UpdatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{"span ledger"} {Code}";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)LeaveDays - (double)Capacity, 2);
    }

    public SpanLedger Copy()
    {
        return new SpanLedger(
            Code,
            Name,
            Capacity,
            Segment,
            UpdatedAt,
            LeaveDays,
            JobCode,
            CreatedAt,
            Locale,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
