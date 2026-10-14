namespace Acme.Workforce.Compensation.BonusPool.OverrideItem;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool ledger is in its life cycle.</summary>
public enum BonusPoolLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A bonus pool ledger. Its code is unique within a tenant.</summary>
public sealed class BonusPoolLedger(string code, string title, string skill, double weight, string description, int quantity, double hours, string segment, string locale, BonusPoolLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Skill { get; set; } = skill;
    public double Weight { get; set; } = weight;
    public string Description { get; set; } = description;
    public int Quantity { get; set; } = quantity;
    public double Hours { get; set; } = hours;
    public string Segment { get; set; } = segment;
    public string Locale { get; set; } = locale;
    public BonusPoolLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool ledger in status open.</summary>
    public static BonusPoolLedger NewBonusPoolLedger(string code, string title)
    {
        return new BonusPoolLedger(code, title, "", 0.0, "", 0, 0.0, "", "", BonusPoolLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolLedgerStatus next)
    {
        switch (Status)
        {
            case BonusPoolLedgerStatus.Open:
                switch (next)
                {
                    case BonusPoolLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case BonusPoolLedgerStatus.InReview:
                switch (next)
                {
                    case BonusPoolLedgerStatus.Reopened:
                    case BonusPoolLedgerStatus.Archived:
                        return true;
                }
                return false;
            case BonusPoolLedgerStatus.Resolved:
                switch (next)
                {
                    case BonusPoolLedgerStatus.Reopened:
                        return true;
                }
                return false;
            case BonusPoolLedgerStatus.Reopened:
                switch (next)
                {
                    case BonusPoolLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code}: {Title}";
    }

    public double AdjustedLoad()
    {
        return Math.Round(Hours - (double)Quantity, 2);
    }

    public BonusPoolLedger Copy()
    {
        return new BonusPoolLedger(
            Code,
            Title,
            Skill,
            Weight,
            Description,
            Quantity,
            Hours,
            Segment,
            Locale,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
