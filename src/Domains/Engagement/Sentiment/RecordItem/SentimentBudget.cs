namespace Acme.Workforce.Engagement.Sentiment.RecordItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a sentiment budget is in its life cycle.</summary>
public enum SentimentBudgetStatus
{
    Idle,
    Warming,
    Ready,
    Draining,
    Stopped,
}

/// <summary>One sentiment budget, with what the planners need to act on it.</summary>
public sealed class SentimentBudget(string code, string label, double discount, string notes, string salaryBand, string department, double floorValue, int attempts, SentimentBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Discount { get; set; } = discount;
    public string Notes { get; set; } = notes;
    public string SalaryBand { get; set; } = salaryBand;
    public string Department { get; set; } = department;
    public double FloorValue { get; set; } = floorValue;
    public int Attempts { get; set; } = attempts;
    public SentimentBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a sentiment budget in status idle.</summary>
    public static SentimentBudget NewSentimentBudget(string code, string label)
    {
        return new SentimentBudget(code, label, 0.0, "", "", "", 0.0, 0, SentimentBudgetStatus.Idle);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SentimentBudgetStatus.Stopped:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SentimentBudgetStatus next)
    {
        switch (Status)
        {
            case SentimentBudgetStatus.Idle:
                switch (next)
                {
                    case SentimentBudgetStatus.Warming:
                        return true;
                }
                return false;
            case SentimentBudgetStatus.Warming:
                switch (next)
                {
                    case SentimentBudgetStatus.Ready:
                        return true;
                }
                return false;
            case SentimentBudgetStatus.Ready:
                switch (next)
                {
                    case SentimentBudgetStatus.Draining:
                    case SentimentBudgetStatus.Stopped:
                        return true;
                }
                return false;
            case SentimentBudgetStatus.Draining:
                return next == SentimentBudgetStatus.Stopped;
        }
        return false;
    }

    public void MoveTo(SentimentBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"sentiment budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double WeightedTotal()
    {
        return Math.Round((double)Attempts - FloorValue, 2);
    }

    public SentimentBudget Copy()
    {
        return new SentimentBudget(
            Code,
            Label,
            Discount,
            Notes,
            SalaryBand,
            Department,
            FloorValue,
            Attempts,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Department);
    }
}
