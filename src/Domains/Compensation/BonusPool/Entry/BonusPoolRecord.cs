namespace Acme.Workforce.Compensation.BonusPool.Entry;

using Acme.Workforce.Kit.Text;

/// <summary>Where a bonus pool record is in its life cycle.</summary>
public enum BonusPoolRecordStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>The record of a bonus pool record and its current status.</summary>
public sealed class BonusPoolRecord(string code, string label, int startsAt, double amount, int endsAt, string notes, string salaryBand, int attempts, BonusPoolRecordStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int StartsAt { get; set; } = startsAt;
    public double Amount { get; set; } = amount;
    public int EndsAt { get; set; } = endsAt;
    public string Notes { get; set; } = notes;
    public string SalaryBand { get; set; } = salaryBand;
    public int Attempts { get; set; } = attempts;
    public BonusPoolRecordStatus Status { get; set; } = status;

    /// <summary>Starts a bonus pool record in status open.</summary>
    public static BonusPoolRecord NewBonusPoolRecord(string code, string label)
    {
        return new BonusPoolRecord(code, label, 0, 0.0, 0, "", "", 0, BonusPoolRecordStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BonusPoolRecordStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BonusPoolRecordStatus next)
    {
        switch (Status)
        {
            case BonusPoolRecordStatus.Open:
                switch (next)
                {
                    case BonusPoolRecordStatus.InReview:
                    case BonusPoolRecordStatus.Archived:
                        return true;
                }
                return false;
            case BonusPoolRecordStatus.InReview:
                return next == BonusPoolRecordStatus.Reopened;
            case BonusPoolRecordStatus.Resolved:
                switch (next)
                {
                    case BonusPoolRecordStatus.Reopened:
                    case BonusPoolRecordStatus.Archived:
                        return true;
                }
                return false;
            case BonusPoolRecordStatus.Reopened:
                switch (next)
                {
                    case BonusPoolRecordStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BonusPoolRecordStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"bonus pool record {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{"bonus pool record"} {Code}";
    }

    public double EffectiveLoad()
    {
        return Math.Round(Amount + (double)Attempts, 2);
    }

    public BonusPoolRecord Copy()
    {
        return new BonusPoolRecord(Code, Label, StartsAt, Amount, EndsAt, Notes, SalaryBand, Attempts, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
