namespace Acme.Workforce.Orgdesign.Reorg.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a global reorg draft is in its life cycle.</summary>
public enum GlobalReorgDraftStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One global reorg draft, with what the planners need to act on it.</summary>
public sealed class GlobalReorgDraft(string code, string title, double budget, double rate, string channel, double margin, int batchSize, int attempts, int quantity, double amount, string department, GlobalReorgDraftStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Budget { get; set; } = budget;
    public double Rate { get; set; } = rate;
    public string Channel { get; set; } = channel;
    public double Margin { get; set; } = margin;
    public int BatchSize { get; set; } = batchSize;
    public int Attempts { get; set; } = attempts;
    public int Quantity { get; set; } = quantity;
    public double Amount { get; set; } = amount;
    public string Department { get; set; } = department;
    public GlobalReorgDraftStatus Status { get; set; } = status;

    /// <summary>Starts a global reorg draft in status idle.</summary>
    public static GlobalReorgDraft NewGlobalReorgDraft(string code, string title)
    {
        return new GlobalReorgDraft(code, title, 0.0, 0.0, "", 0.0, 0, 0, 0, 0.0, "", GlobalReorgDraftStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case GlobalReorgDraftStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(GlobalReorgDraftStatus next)
    {
        switch (Status)
        {
            case GlobalReorgDraftStatus.Idle:
                switch (next)
                {
                    case GlobalReorgDraftStatus.Ready:
                        return true;
                }
                return false;
            case GlobalReorgDraftStatus.Warming:
                switch (next)
                {
                    case GlobalReorgDraftStatus.Ready:
                        return true;
                }
                return false;
            case GlobalReorgDraftStatus.Ready:
                switch (next)
                {
                    case GlobalReorgDraftStatus.Stopped:
                        return true;
                }
                return false;
            case GlobalReorgDraftStatus.Draining:
                switch (next)
                {
                    case GlobalReorgDraftStatus.Stopped:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(GlobalReorgDraftStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"global reorg draft {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Title}";
    }

    public double ProjectedTotal()
    {
        return Math.Round(Margin + Amount, 2);
    }

    public GlobalReorgDraft Copy()
    {
        return new GlobalReorgDraft(
            Code,
            Title,
            Budget,
            Rate,
            Channel,
            Margin,
            BatchSize,
            Attempts,
            Quantity,
            Amount,
            Department,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Channel);
    }
}
