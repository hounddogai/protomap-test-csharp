namespace Acme.Workforce.Payrollops.TaxForm.FormGroup;

using Acme.Workforce.Kit.Text;

/// <summary>Where a pending tax form override is in its life cycle.</summary>
public enum PendingTaxFormOverrideStatus
{
    Planned,
    Scheduled,
    InProgress,
    Completed,
    Cancelled,
}

/// <summary>A pending tax form override as the tax form team keeps it.</summary>
public sealed class PendingTaxFormOverride(string code, string title, bool archived, double amount, Dictionary<string, string> attributes, double floorValue, double budget, PendingTaxFormOverrideStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public bool Archived { get; set; } = archived;
    public double Amount { get; set; } = amount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public double FloorValue { get; set; } = floorValue;
    public double Budget { get; set; } = budget;
    public PendingTaxFormOverrideStatus Status { get; set; } = status;

    /// <summary>Starts a pending tax form override in status planned.</summary>
    public static PendingTaxFormOverride NewPendingTaxFormOverride(string code, string title)
    {
        return new PendingTaxFormOverride(
            code,
            title,
            false,
            0.0,
            new Dictionary<string, string>(),
            0.0,
            0.0,
            PendingTaxFormOverrideStatus.Planned);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PendingTaxFormOverrideStatus.Cancelled:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PendingTaxFormOverrideStatus next)
    {
        switch (Status)
        {
            case PendingTaxFormOverrideStatus.Planned:
                switch (next)
                {
                    case PendingTaxFormOverrideStatus.Scheduled:
                    case PendingTaxFormOverrideStatus.Completed:
                        return true;
                }
                return false;
            case PendingTaxFormOverrideStatus.Scheduled:
                switch (next)
                {
                    case PendingTaxFormOverrideStatus.InProgress:
                    case PendingTaxFormOverrideStatus.Cancelled:
                        return true;
                }
                return false;
            case PendingTaxFormOverrideStatus.InProgress:
                return next == PendingTaxFormOverrideStatus.Completed || next == PendingTaxFormOverrideStatus.Cancelled;
            case PendingTaxFormOverrideStatus.Completed:
                switch (next)
                {
                    case PendingTaxFormOverrideStatus.Cancelled:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PendingTaxFormOverrideStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"pending tax form override {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"pending tax form override"} {Code}";
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

    public double NetValue()
    {
        return Math.Round(Amount * FloorValue, 2);
    }

    public PendingTaxFormOverride Copy()
    {
        return new PendingTaxFormOverride(Code, Title, Archived, Amount, Attributes, FloorValue, Budget, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
