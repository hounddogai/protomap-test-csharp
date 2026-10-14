namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return policy is in its life cycle.</summary>
public enum AssetReturnPolicyStatus
{
    Open,
    InReview,
    Resolved,
    Reopened,
    Archived,
}

/// <summary>A asset return policy as the asset return team keeps it.</summary>
public sealed class AssetReturnPolicy(string code, string label, int limit, string grade, string salaryBand, double threshold, int tenureMonths, string timezone, double floorValue, string employeeId, AssetReturnPolicyStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public int Limit { get; set; } = limit;
    public string Grade { get; set; } = grade;
    public string SalaryBand { get; set; } = salaryBand;
    public double Threshold { get; set; } = threshold;
    public int TenureMonths { get; set; } = tenureMonths;
    public string Timezone { get; set; } = timezone;
    public double FloorValue { get; set; } = floorValue;
    public string EmployeeId { get; set; } = employeeId;
    public AssetReturnPolicyStatus Status { get; set; } = status;

    /// <summary>Starts a asset return policy in status open.</summary>
    public static AssetReturnPolicy NewAssetReturnPolicy(string code, string label)
    {
        return new AssetReturnPolicy(code, label, 0, "", "", 0.0, 0, "", 0.0, "", AssetReturnPolicyStatus.Open);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnPolicyStatus.Archived:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnPolicyStatus next)
    {
        switch (Status)
        {
            case AssetReturnPolicyStatus.Open:
                return next == AssetReturnPolicyStatus.Resolved || next == AssetReturnPolicyStatus.Archived;
            case AssetReturnPolicyStatus.InReview:
                switch (next)
                {
                    case AssetReturnPolicyStatus.Reopened:
                        return true;
                }
                return false;
            case AssetReturnPolicyStatus.Resolved:
                switch (next)
                {
                    case AssetReturnPolicyStatus.Reopened:
                        return true;
                }
                return false;
            case AssetReturnPolicyStatus.Reopened:
                return next == AssetReturnPolicyStatus.Archived;
        }
        return false;
    }

    public void MoveTo(AssetReturnPolicyStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return policy {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedExposure()
    {
        return Math.Round(Threshold + (double)Limit, 2);
    }

    public AssetReturnPolicy Copy()
    {
        return new AssetReturnPolicy(
            Code,
            Label,
            Limit,
            Grade,
            SalaryBand,
            Threshold,
            TenureMonths,
            Timezone,
            FloorValue,
            EmployeeId,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
