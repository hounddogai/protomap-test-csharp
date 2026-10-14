namespace Acme.Workforce.Engagement.Sentiment.Proposal;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scheduled sentiment ledger is in its life cycle.</summary>
public enum ScheduledSentimentLedgerStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scheduled sentiment ledger. Its code is unique within a tenant.</summary>
public sealed class ScheduledSentimentLedger(string code, string label, string channel, int tenureMonths, string description, string managerId, double minimum, string country, ScheduledSentimentLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Channel { get; set; } = channel;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Description { get; set; } = description;
    public string ManagerId { get; set; } = managerId;
    public double Minimum { get; set; } = minimum;
    public string Country { get; set; } = country;
    public ScheduledSentimentLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a scheduled sentiment ledger in status new.</summary>
    public static ScheduledSentimentLedger NewScheduledSentimentLedger(string code, string label)
    {
        return new ScheduledSentimentLedger(code, label, "", 0, "", "", 0.0, "", ScheduledSentimentLedgerStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScheduledSentimentLedgerStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScheduledSentimentLedgerStatus next)
    {
        switch (Status)
        {
            case ScheduledSentimentLedgerStatus.New:
                return next == ScheduledSentimentLedgerStatus.Running || next == ScheduledSentimentLedgerStatus.Done;
            case ScheduledSentimentLedgerStatus.Queued:
                return next == ScheduledSentimentLedgerStatus.Running || next == ScheduledSentimentLedgerStatus.Failed;
            case ScheduledSentimentLedgerStatus.Running:
                switch (next)
                {
                    case ScheduledSentimentLedgerStatus.Failed:
                        return true;
                }
                return false;
            case ScheduledSentimentLedgerStatus.Done:
                switch (next)
                {
                    case ScheduledSentimentLedgerStatus.Failed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(ScheduledSentimentLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scheduled sentiment ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveTotal()
    {
        return Math.Round(Minimum + (double)TenureMonths, 2);
    }

    public ScheduledSentimentLedger Copy()
    {
        return new ScheduledSentimentLedger(
            Code,
            Label,
            Channel,
            TenureMonths,
            Description,
            ManagerId,
            Minimum,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
