namespace Acme.Workforce.Assets.AssetReturn.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a asset return summary is in its life cycle.</summary>
public enum AssetReturnSummaryStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>A asset return summary. Its code is unique within a tenant.</summary>
public sealed class AssetReturnSummary(string code, string label, string segment, double cost, string employeeId, double score, bool visible, string salaryBand, AssetReturnSummaryStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Segment { get; set; } = segment;
    public double Cost { get; set; } = cost;
    public string EmployeeId { get; set; } = employeeId;
    public double Score { get; set; } = score;
    public bool Visible { get; set; } = visible;
    public string SalaryBand { get; set; } = salaryBand;
    public AssetReturnSummaryStatus Status { get; set; } = status;

    /// <summary>Starts a asset return summary in status proposed.</summary>
    public static AssetReturnSummary NewAssetReturnSummary(string code, string label)
    {
        return new AssetReturnSummary(code, label, "", 0.0, "", 0.0, false, "", AssetReturnSummaryStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case AssetReturnSummaryStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(AssetReturnSummaryStatus next)
    {
        switch (Status)
        {
            case AssetReturnSummaryStatus.Proposed:
                return next == AssetReturnSummaryStatus.Approved || next == AssetReturnSummaryStatus.Withdrawn;
            case AssetReturnSummaryStatus.Approved:
                return next == AssetReturnSummaryStatus.Withdrawn;
            case AssetReturnSummaryStatus.Rejected:
                switch (next)
                {
                    case AssetReturnSummaryStatus.Withdrawn:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(AssetReturnSummaryStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"asset return summary {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code} ({Label})";
    }

    public double AdjustedTotal()
    {
        return Math.Round(Score - Cost, 2);
    }

    public AssetReturnSummary Copy()
    {
        return new AssetReturnSummary(Code, Label, Segment, Cost, EmployeeId, Score, Visible, SalaryBand, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
