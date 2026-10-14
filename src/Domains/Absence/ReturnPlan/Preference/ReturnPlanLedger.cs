namespace Acme.Workforce.Absence.ReturnPlan.Preference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a return plan ledger is in its life cycle.</summary>
public enum ReturnPlanLedgerStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A return plan ledger as the return plan team keeps it.</summary>
public sealed class ReturnPlanLedger(string code, string title, double threshold, string unit, double ceiling, string country, Dictionary<string, string> attributes, string category, bool pinned, string team, int endsAt, ReturnPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Threshold { get; set; } = threshold;
    public string Unit { get; set; } = unit;
    public double Ceiling { get; set; } = ceiling;
    public string Country { get; set; } = country;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Category { get; set; } = category;
    public bool Pinned { get; set; } = pinned;
    public string Team { get; set; } = team;
    public int EndsAt { get; set; } = endsAt;
    public ReturnPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a return plan ledger in status proposed.</summary>
    public static ReturnPlanLedger NewReturnPlanLedger(string code, string title)
    {
        return new ReturnPlanLedger(
            code,
            title,
            0.0,
            "",
            0.0,
            "",
            new Dictionary<string, string>(),
            "",
            false,
            "",
            0,
            ReturnPlanLedgerStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ReturnPlanLedgerStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ReturnPlanLedgerStatus next)
    {
        switch (Status)
        {
            case ReturnPlanLedgerStatus.Proposed:
                return next == ReturnPlanLedgerStatus.Approved;
            case ReturnPlanLedgerStatus.Approved:
                switch (next)
                {
                    case ReturnPlanLedgerStatus.Rejected:
                    case ReturnPlanLedgerStatus.Withdrawn:
                        return true;
                }
                return false;
            case ReturnPlanLedgerStatus.Rejected:
                return next == ReturnPlanLedgerStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ReturnPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"return plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string Label()
    {
        return $"{Code}: {Title}";
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

    public double WeightedExposure()
    {
        return Math.Round((double)EndsAt - Ceiling, 2);
    }

    public ReturnPlanLedger Copy()
    {
        return new ReturnPlanLedger(
            Code,
            Title,
            Threshold,
            Unit,
            Ceiling,
            Country,
            Attributes,
            Category,
            Pinned,
            Team,
            EndsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
