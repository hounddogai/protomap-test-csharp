namespace Acme.Workforce.Orgdesign.Headcount.Allocation;

using Acme.Workforce.Kit.Text;

/// <summary>Where a regional headcount budget is in its life cycle.</summary>
public enum RegionalHeadcountBudgetStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>A regional headcount budget as the headcount team keeps it.</summary>
public sealed class RegionalHeadcountBudget(string code, string name, string segment, double score, bool archived, int revision, string description, string summary, string ownerId, string costCenter, int startsAt, RegionalHeadcountBudgetStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public string Segment { get; set; } = segment;
    public double Score { get; set; } = score;
    public bool Archived { get; set; } = archived;
    public int Revision { get; set; } = revision;
    public string Description { get; set; } = description;
    public string Summary { get; set; } = summary;
    public string OwnerId { get; set; } = ownerId;
    public string CostCenter { get; set; } = costCenter;
    public int StartsAt { get; set; } = startsAt;
    public RegionalHeadcountBudgetStatus Status { get; set; } = status;

    /// <summary>Starts a regional headcount budget in status created.</summary>
    public static RegionalHeadcountBudget NewRegionalHeadcountBudget(string code, string name)
    {
        return new RegionalHeadcountBudget(
            code,
            name,
            "",
            0.0,
            false,
            0,
            "",
            "",
            "",
            "",
            0,
            RegionalHeadcountBudgetStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case RegionalHeadcountBudgetStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(RegionalHeadcountBudgetStatus next)
    {
        switch (Status)
        {
            case RegionalHeadcountBudgetStatus.Created:
                switch (next)
                {
                    case RegionalHeadcountBudgetStatus.Retired:
                        return true;
                }
                return false;
            case RegionalHeadcountBudgetStatus.Validated:
                return next == RegionalHeadcountBudgetStatus.Published;
            case RegionalHeadcountBudgetStatus.Published:
                switch (next)
                {
                    case RegionalHeadcountBudgetStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(RegionalHeadcountBudgetStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"regional headcount budget {Code} cannot move from {Status} to {next}");
        }
        Status = next;
        StartsAt = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Revision += 1;
    }

    public string Label()
    {
        return $"{Code} ({Name})";
    }

    public double AdjustedExposure()
    {
        return Math.Round((double)StartsAt - (double)Revision, 2);
    }

    public RegionalHeadcountBudget Copy()
    {
        return new RegionalHeadcountBudget(
            Code,
            Name,
            Segment,
            Score,
            Archived,
            Revision,
            Description,
            Summary,
            OwnerId,
            CostCenter,
            StartsAt,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Segment);
    }
}
