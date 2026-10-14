namespace Acme.Workforce.Absence.ReturnPlan.SeasonalPlanManifest;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional return plan item is in its life cycle.</summary>
public enum RegionalReturnPlanItemStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A regional return plan item. Its code is unique within a tenant.</summary>
public sealed class RegionalReturnPlanItem(string code, string name, double maximum, int limit, double weight, double score, string reviewer, RegionalReturnPlanItemStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public double Maximum { get; set; } = maximum;
    public int Limit { get; set; } = limit;
    public double Weight { get; set; } = weight;
    public double Score { get; set; } = score;
    public string Reviewer { get; set; } = reviewer;
    public RegionalReturnPlanItemStatus Status { get; set; } = status;

    /// <summary>Starts a regional return plan item in status draft.</summary>
    public static RegionalReturnPlanItem NewRegionalReturnPlanItem(string code, string name)
    {
        return new RegionalReturnPlanItem(code, name, 0.0, 0, 0.0, 0.0, "", RegionalReturnPlanItemStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalReturnPlanItemStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalReturnPlanItemStatus next)
    {
        switch (Status)
        {
            case RegionalReturnPlanItemStatus.Draft:
                return next == RegionalReturnPlanItemStatus.Pending;
            case RegionalReturnPlanItemStatus.Pending:
                switch (next)
                {
                    case RegionalReturnPlanItemStatus.Closed:
                        return true;
                }
                return false;
            case RegionalReturnPlanItemStatus.Active:
                switch (next)
                {
                    case RegionalReturnPlanItemStatus.Closed:
                        return true;
                }
                return false;
            case RegionalReturnPlanItemStatus.Suspended:
                return next == RegionalReturnPlanItemStatus.Closed;
        }
        return false;
    }

    public void MoveTo(RegionalReturnPlanItemStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional return plan item {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} [{Name}]";
    }

    public double NetLoad()
    {
        return Math.Round((double)Limit - Score, 2);
    }

    public RegionalReturnPlanItem Copy()
    {
        return new RegionalReturnPlanItem(Code, Name, Maximum, Limit, Weight, Score, Reviewer, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
