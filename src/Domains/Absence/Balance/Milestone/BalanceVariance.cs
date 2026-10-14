namespace Acme.Workforce.Absence.Balance.Milestone;

using Acme.Workforce.Kit.Text;

/// <summary>Where a balance variance is in its life cycle.</summary>
public enum BalanceVarianceStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A balance variance as the balance team keeps it.</summary>
public sealed class BalanceVariance(string code, string name, double maximum, double amount, Dictionary<string, string> attributes, string summary, BalanceVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public double Amount { get; set; } = amount;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Summary { get; set; } = summary;
    public BalanceVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a balance variance in status proposed.</summary>
    public static BalanceVariance NewBalanceVariance(string code, string name)
    {
        return new BalanceVariance(
            code,
            name,
            0.0,
            0.0,
            new Dictionary<string, string>(),
            "",
            BalanceVarianceStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case BalanceVarianceStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(BalanceVarianceStatus next)
    {
        switch (Status)
        {
            case BalanceVarianceStatus.Proposed:
                switch (next)
                {
                    case BalanceVarianceStatus.Rejected:
                    case BalanceVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case BalanceVarianceStatus.Approved:
                switch (next)
                {
                    case BalanceVarianceStatus.Rejected:
                    case BalanceVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
            case BalanceVarianceStatus.Rejected:
                switch (next)
                {
                    case BalanceVarianceStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(BalanceVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"balance variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"balance variance"} {Code}";
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

    public double EffectiveValue()
    {
        return Math.Round(Maximum + Amount, 2);
    }

    public BalanceVariance Copy()
    {
        return new BalanceVariance(Code, Name, Maximum, Amount, Attributes, Summary, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
