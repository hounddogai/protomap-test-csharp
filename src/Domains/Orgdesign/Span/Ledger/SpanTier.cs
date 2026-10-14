namespace Acme.Workforce.Orgdesign.Span.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span tier is in its life cycle.</summary>
public enum SpanTierStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A span tier. Its code is unique within a tenant.</summary>
public sealed class SpanTier(string code, string name, double hours, string reviewer, string externalRef, string department, double rate, string team, string currency, int quantity, bool approved, SpanTierStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Hours { get; set; } = hours;
    public string Reviewer { get; set; } = reviewer;
    public string ExternalRef { get; set; } = externalRef;
    public string Department { get; set; } = department;
    public double Rate { get; set; } = rate;
    public string Team { get; set; } = team;
    public string Currency { get; set; } = currency;
    public int Quantity { get; set; } = quantity;
    public bool Approved { get; set; } = approved;
    public SpanTierStatus Status { get; set; } = status;

    /// <summary>Starts a span tier in status draft.</summary>
    public static SpanTier NewSpanTier(string code, string name)
    {
        return new SpanTier(code, name, 0.0, "", "", "", 0.0, "", "", 0, false, SpanTierStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanTierStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanTierStatus next)
    {
        switch (Status)
        {
            case SpanTierStatus.Draft:
                return next == SpanTierStatus.Pending || next == SpanTierStatus.Suspended;
            case SpanTierStatus.Pending:
                switch (next)
                {
                    case SpanTierStatus.Active:
                    case SpanTierStatus.Suspended:
                        return true;
                }
                return false;
            case SpanTierStatus.Active:
                return next == SpanTierStatus.Suspended || next == SpanTierStatus.Closed;
            case SpanTierStatus.Suspended:
                return next == SpanTierStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SpanTierStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span tier {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Hours * Rate, 2);
    }

    public SpanTier Copy()
    {
        return new SpanTier(
            Code,
            Name,
            Hours,
            Reviewer,
            ExternalRef,
            Department,
            Rate,
            Team,
            Currency,
            Quantity,
            Approved,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
