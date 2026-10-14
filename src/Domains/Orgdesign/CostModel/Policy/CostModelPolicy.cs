namespace Acme.Workforce.Orgdesign.CostModel.Policy;

using Acme.Workforce.Kit.Text;

/// <summary>Where a cost model policy is in its life cycle.</summary>
public enum CostModelPolicyStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A cost model policy. Its code is unique within a tenant.</summary>
public sealed class CostModelPolicy(string code, string label, double threshold, string unit, int endsAt, double hours, CostModelPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public double Threshold { get; set; } = threshold;
    public string Unit { get; set; } = unit;
    public int EndsAt { get; set; } = endsAt;
    public double Hours { get; set; } = hours;
    public CostModelPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a cost model policy in status created.</summary>
    public static CostModelPolicy NewCostModelPolicy(string code, string label)
    {
        return new CostModelPolicy(code, label, 0.0, "", 0, 0.0, CostModelPolicyStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case CostModelPolicyStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(CostModelPolicyStatus next)
    {
        switch (Status)
        {
            case CostModelPolicyStatus.Created:
                switch (next)
                {
                    case CostModelPolicyStatus.Published:
                        return true;
                }
                return false;
            case CostModelPolicyStatus.Validated:
                switch (next)
                {
                    case CostModelPolicyStatus.Published:
                        return true;
                }
                return false;
            case CostModelPolicyStatus.Published:
                return next == CostModelPolicyStatus.Retired;
        }
        return false;
    }

    public void MoveTo(CostModelPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"cost model policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        EndsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} [{Label}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)EndsAt * Hours, 2);
    }

    public CostModelPolicy Copy()
    {
        return new CostModelPolicy(Code, Label, Threshold, Unit, EndsAt, Hours, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Unit);
    }
}
