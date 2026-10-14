namespace Acme.Workforce.Talent.CareerPath.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a career path ledger is in its life cycle.</summary>
public enum CareerPathLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A career path ledger. Its code is unique within a tenant.</summary>
public sealed class CareerPathLedger(string code, string label, string jobCode, double weight, int startsAt, string category, string name, CareerPathLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string JobCode { get; set; } = jobCode;
    public double Weight { get; set; } = weight;
    public int StartsAt { get; set; } = startsAt;
    public string Category { get; set; } = category;
    public string Name { get; set; } = name;
    public CareerPathLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a career path ledger in status open.</summary>
    public static CareerPathLedger NewCareerPathLedger(string code, string label)
    {
        return new CareerPathLedger(code, label, "", 0.0, 0, "", "", CareerPathLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CareerPathLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CareerPathLedgerStatus next)
    {
        switch (Status)
        {
            case CareerPathLedgerStatus.Open:
                return next == CareerPathLedgerStatus.Reopened || next == CareerPathLedgerStatus.Archived;
            case CareerPathLedgerStatus.InReview:
                return next == CareerPathLedgerStatus.Resolved || next == CareerPathLedgerStatus.Reopened;
            case CareerPathLedgerStatus.Resolved:
                return next == CareerPathLedgerStatus.Archived;
            case CareerPathLedgerStatus.Reopened:
                switch (next)
                {
                    case CareerPathLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(CareerPathLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"career path ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double AdjustedTotal()
    {
        return Math.Round((double)StartsAt - Weight, 2);
    }

    public CareerPathLedger Copy()
    {
        return new CareerPathLedger(Code, Label, JobCode, Weight, StartsAt, Category, Name, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Category);
    }
}
