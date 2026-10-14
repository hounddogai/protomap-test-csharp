namespace Acme.Workforce.Compensation.EquityGrant.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a equity grant summary is in its life cycle.</summary>
public enum EquityGrantSummaryStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One equity grant summary, with what the planners need to act on it.</summary>
public sealed class EquityGrantSummary(string code, string label, double amount, string unit, string checksum, double hours, double ceiling, EquityGrantSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Amount { get; set; } = amount;
    public string Unit { get; set; } = unit;
    public string Checksum { get; set; } = checksum;
    public double Hours { get; set; } = hours;
    public double Ceiling { get; set; } = ceiling;
    public EquityGrantSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a equity grant summary in status created.</summary>
    public static EquityGrantSummary NewEquityGrantSummary(string code, string label)
    {
        return new EquityGrantSummary(code, label, 0.0, "", "", 0.0, 0.0, EquityGrantSummaryStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case EquityGrantSummaryStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(EquityGrantSummaryStatus next)
    {
        switch (Status)
        {
            case EquityGrantSummaryStatus.Created:
                switch (next)
                {
                    case EquityGrantSummaryStatus.Validated:
                    case EquityGrantSummaryStatus.Published:
                        return true;
                }
                return false;
            case EquityGrantSummaryStatus.Validated:
                switch (next)
                {
                    case EquityGrantSummaryStatus.Retired:
                        return true;
                }
                return false;
            case EquityGrantSummaryStatus.Published:
                return next == EquityGrantSummaryStatus.Retired;
        }
        return false;
    }

    public void MoveTo(EquityGrantSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"equity grant summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double NetLoad()
    {
        return Math.Round(Ceiling - Hours, 2);
    }

    public EquityGrantSummary Copy()
    {
        return new EquityGrantSummary(Code, Label, Amount, Unit, Checksum, Hours, Ceiling, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
