namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a reorg summary is in its life cycle.</summary>
public enum ReorgSummaryStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A reorg summary as the reorg team keeps it.</summary>
public sealed class ReorgSummary(string code, string label, string phone, double margin, double rate, ReorgSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Phone { get; set; } = phone;
    public double Margin { get; set; } = margin;
    public double Rate { get; set; } = rate;
    public ReorgSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a reorg summary in status new.</summary>
    public static ReorgSummary NewReorgSummary(string code, string label)
    {
        return new ReorgSummary(code, label, "", 0.0, 0.0, ReorgSummaryStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReorgSummaryStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReorgSummaryStatus next)
    {
        switch (Status)
        {
            case ReorgSummaryStatus.New:
                return next == ReorgSummaryStatus.Failed;
            case ReorgSummaryStatus.Queued:
                return next == ReorgSummaryStatus.Failed;
            case ReorgSummaryStatus.Running:
                return next == ReorgSummaryStatus.Done;
            case ReorgSummaryStatus.Done:
                return next == ReorgSummaryStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ReorgSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"reorg summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Margin + Rate, 2);
    }

    public ReorgSummary Copy()
    {
        return new ReorgSummary(Code, Label, Phone, Margin, Rate, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
