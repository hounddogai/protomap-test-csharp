namespace Acme.Workforce.Absence.Policy.PolicyPreference;

using Acme.Workforce.Kit.Text;

/// <summary>Where a policy variance is in its life cycle.</summary>
public enum PolicyVarianceStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>The record of a policy variance and its current status.</summary>
public sealed class PolicyVariance(string code, string label, string unit, string currency, string team, int quantity, PolicyVarianceStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Unit { get; set; } = unit;
    public string Currency { get; set; } = currency;
    public string Team { get; set; } = team;
    public int Quantity { get; set; } = quantity;
    public PolicyVarianceStatus Status { get; set; } = status;

    /// <summary>Starts a policy variance in status created.</summary>
    public static PolicyVariance NewPolicyVariance(string code, string label)
    {
        return new PolicyVariance(code, label, "", "", "", 0, PolicyVarianceStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case PolicyVarianceStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(PolicyVarianceStatus next)
    {
        switch (Status)
        {
            case PolicyVarianceStatus.Created:
                switch (next)
                {
                    case PolicyVarianceStatus.Published:
                    case PolicyVarianceStatus.Retired:
                        return true;
                }
                return false;
            case PolicyVarianceStatus.Validated:
                switch (next)
                {
                    case PolicyVarianceStatus.Published:
                    case PolicyVarianceStatus.Retired:
                        return true;
                }
                return false;
            case PolicyVarianceStatus.Published:
                switch (next)
                {
                    case PolicyVarianceStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(PolicyVarianceStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"policy variance {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double MeasuredValue()
    {
        return (double)Quantity;
    }

    public PolicyVariance Copy()
    {
        return new PolicyVariance(Code, Label, Unit, Currency, Team, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
