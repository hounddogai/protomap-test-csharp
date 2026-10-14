namespace Acme.Workforce.Orgdesign.CostModel.ModelSummary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal cost model milestone is in its life cycle.</summary>
public enum SeasonalCostModelMilestoneStatus
{
    Draft,
    Pending,
    Active,
    Suspended,
    Closed,
}

/// <summary>A seasonal cost model milestone. Its code is unique within a tenant.</summary>
public sealed class SeasonalCostModelMilestone(string code, string label, string location, string summary, int graceDays, double amount, int createdAt, SeasonalCostModelMilestoneStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Location { get; set; } = location;
    public string Summary { get; set; } = summary;
    public int GraceDays { get; set; } = graceDays;
    public double Amount { get; set; } = amount;
    public int CreatedAt { get; set; } = createdAt;
    public SeasonalCostModelMilestoneStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal cost model milestone in status draft.</summary>
    public static SeasonalCostModelMilestone NewSeasonalCostModelMilestone(string code, string label)
    {
        return new SeasonalCostModelMilestone(
            code,
            label,
            "",
            "",
            0,
            0.0,
            0,
            SeasonalCostModelMilestoneStatus.Draft);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalCostModelMilestoneStatus.Closed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalCostModelMilestoneStatus next)
    {
        switch (Status)
        {
            case SeasonalCostModelMilestoneStatus.Draft:
                return next == SeasonalCostModelMilestoneStatus.Active;
            case SeasonalCostModelMilestoneStatus.Pending:
                return next == SeasonalCostModelMilestoneStatus.Active;
            case SeasonalCostModelMilestoneStatus.Active:
                switch (next)
                {
                    case SeasonalCostModelMilestoneStatus.Suspended:
                        return true;
                }
                return false;
            case SeasonalCostModelMilestoneStatus.Suspended:
                switch (next)
                {
                    case SeasonalCostModelMilestoneStatus.Closed:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalCostModelMilestoneStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal cost model milestone {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        CreatedAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public string DisplayLabel()
    {
        return $"{Code} / {Label}";
    }

    public double EffectiveVolume()
    {
        return Math.Round((double)CreatedAt + (double)GraceDays, 2);
    }

    public SeasonalCostModelMilestone Copy()
    {
        return new SeasonalCostModelMilestone(Code, Label, Location, Summary, GraceDays, Amount, CreatedAt, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Label);
    }
}
