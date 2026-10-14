namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Kit.Text;

/// <summary>Where a warranty policy is in its life cycle.</summary>
public enum WarrantyPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A warranty policy. Its code is unique within a tenant.</summary>
public sealed class WarrantyPolicy(string code, string label, double budget, string description, int limit, WarrantyPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Budget { get; set; } = budget;
    public string Description { get; set; } = description;
    public int Limit { get; set; } = limit;
    public WarrantyPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a warranty policy in status created.</summary>
    public static WarrantyPolicy NewWarrantyPolicy(string code, string label)
    {
        return new WarrantyPolicy(code, label, 0.0, "", 0, WarrantyPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WarrantyPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WarrantyPolicyStatus next)
    {
        switch (Status)
        {
            case WarrantyPolicyStatus.Created:
                switch (next)
                {
                    case WarrantyPolicyStatus.Published:
                        return true;
                }
                return false;
            case WarrantyPolicyStatus.Validated:
                switch (next)
                {
                    case WarrantyPolicyStatus.Published:
                    case WarrantyPolicyStatus.Retired:
                        return true;
                }
                return false;
            case WarrantyPolicyStatus.Published:
                return next == WarrantyPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(WarrantyPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"warranty policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{"warranty policy"} {Code}";
    }

    public double WeightedLoad()
    {
        return Math.Round((double)Limit - Budget, 2);
    }

    public WarrantyPolicy Copy()
    {
        return new WarrantyPolicy(Code, Label, Budget, Description, Limit, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
