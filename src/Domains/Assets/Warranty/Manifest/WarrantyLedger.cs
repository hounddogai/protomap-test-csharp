namespace Acme.Workforce.Assets.Warranty.Manifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty ledger is in its life cycle.</summary>
public enum WarrantyLedgerStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>One warranty ledger, with what the planners need to act on it.</summary>
public sealed class WarrantyLedger(string code, string title, double score, string salaryBand, double minimum, string description, Dictionary<string, string> attributes, WarrantyLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Score { get; set; } = score;
    public string SalaryBand { get; set; } = salaryBand;
    public double Minimum { get; set; } = minimum;
    public string Description { get; set; } = description;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public WarrantyLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a warranty ledger in status open.</summary>
    public static WarrantyLedger NewWarrantyLedger(string code, string title)
    {
        return new WarrantyLedger(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            WarrantyLedgerStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyLedgerStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyLedgerStatus next)
    {
        switch (Status)
        {
            case WarrantyLedgerStatus.Open:
                switch (next)
                {
                    case WarrantyLedgerStatus.Reopened:
                    case WarrantyLedgerStatus.Archived:
                        return true;
                }
                return false;
            case WarrantyLedgerStatus.InReview:
                return next == WarrantyLedgerStatus.Archived;
            case WarrantyLedgerStatus.Resolved:
                return next == WarrantyLedgerStatus.Reopened || next == WarrantyLedgerStatus.Archived;
            case WarrantyLedgerStatus.Reopened:
                switch (next)
                {
                    case WarrantyLedgerStatus.Archived:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(WarrantyLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"warranty ledger"} {Code}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double ProjectedValue()
    {
        return Math.Round(Minimum * Score, 2);
    }

    public WarrantyLedger Copy()
    {
        return new WarrantyLedger(Code, Title, Score, SalaryBand, Minimum, Description, Attributes, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
