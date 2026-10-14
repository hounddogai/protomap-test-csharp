namespace Acme.Workforce.Talent.SuccessionPlan.Ledger;

using Acme.Workforce.Kit.Text;

/// <summary>Where a succession plan ledger is in its life cycle.</summary>
public enum SuccessionPlanLedgerStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a succession plan ledger and its current status.</summary>
public sealed class SuccessionPlanLedger(string code, string title, string locale, double floorValue, Dictionary<string, string> attributes, bool approved, SuccessionPlanLedgerStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public string Locale { get; set; } = locale;
    public double FloorValue { get; set; } = floorValue;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public bool Approved { get; set; } = approved;
    public SuccessionPlanLedgerStatus Status { get; set; } = status;

    /// <summary>Starts a succession plan ledger in status created.</summary>
    public static SuccessionPlanLedger NewSuccessionPlanLedger(string code, string title)
    {
        return new SuccessionPlanLedger(
            code,
            title,
            "",
            0.0,
            new Dictionary<string, string>(),
            false,
            SuccessionPlanLedgerStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SuccessionPlanLedgerStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SuccessionPlanLedgerStatus next)
    {
        switch (Status)
        {
            case SuccessionPlanLedgerStatus.Created:
                switch (next)
                {
                    case SuccessionPlanLedgerStatus.Validated:
                    case SuccessionPlanLedgerStatus.Retired:
                        return true;
                }
                return false;
            case SuccessionPlanLedgerStatus.Validated:
                switch (next)
                {
                    case SuccessionPlanLedgerStatus.Retired:
                        return true;
                }
                return false;
            case SuccessionPlanLedgerStatus.Published:
                switch (next)
                {
                    case SuccessionPlanLedgerStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SuccessionPlanLedgerStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"succession plan ledger {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Title}]";
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

    public double MeasuredValue()
    {
        return FloorValue;
    }

    public SuccessionPlanLedger Copy()
    {
        return new SuccessionPlanLedger(Code, Title, Locale, FloorValue, Attributes, Approved, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Locale);
    }
}
