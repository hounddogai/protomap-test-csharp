namespace Acme.Workforce.Orgdesign.Span.Draft;

using Acme.Workforce.Kit.Text;

/// <summary>Where a span summary is in its life cycle.</summary>
public enum SpanSummaryStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>One span summary, with what the planners need to act on it.</summary>
public sealed class SpanSummary(string code, string label, int limit, string costCenter, string managerId, double discount, SpanSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string CostCenter { get; set; } = costCenter;
    public string ManagerId { get; set; } = managerId;
    public double Discount { get; set; } = discount;
    public SpanSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a span summary in status draft.</summary>
    public static SpanSummary NewSpanSummary(string code, string label)
    {
        return new SpanSummary(code, label, 0, "", "", 0.0, SpanSummaryStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SpanSummaryStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SpanSummaryStatus next)
    {
        switch (Status)
        {
            case SpanSummaryStatus.Draft:
                return next == SpanSummaryStatus.Closed;
            case SpanSummaryStatus.Pending:
                return next == SpanSummaryStatus.Active || next == SpanSummaryStatus.Closed;
            case SpanSummaryStatus.Active:
                return next == SpanSummaryStatus.Suspended || next == SpanSummaryStatus.Closed;
            case SpanSummaryStatus.Suspended:
                return next == SpanSummaryStatus.Closed;
        }
        return false;
    }

    public void MoveTo(SpanSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"span summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"span summary"} {Code}";
    }

    public double EffectiveTotal()
    {
        return Math.Round((double)Limit - Discount, 2);
    }

    public SpanSummary Copy()
    {
        return new SpanSummary(Code, Label, Limit, CostCenter, ManagerId, Discount, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
