namespace Acme.Workforce.Orgdesign.Scenario.Summary;

using Acme.Workforce.Kit.Text;

/// <summary>Where a seasonal scenario request is in its life cycle.</summary>
public enum SeasonalScenarioRequestStatus
{
    Created,
    Validated,
    Published,
    Retired,
}

/// <summary>One seasonal scenario request, with what the planners need to act on it.</summary>
public sealed class SeasonalScenarioRequest(string code, string label, string description, bool approved, double ratio, string region, bool archived, double fte, string grade, int priority, SeasonalScenarioRequestStatus status)
{
    public string Code { get; set; } = code;
    public string Label { get; set; } = label;
    public string Description { get; set; } = description;
    public bool Approved { get; set; } = approved;
    public double Ratio { get; set; } = ratio;
    public string Region { get; set; } = region;
    public bool Archived { get; set; } = archived;
    public double Fte { get; set; } = fte;
    public string Grade { get; set; } = grade;
    public int Priority { get; set; } = priority;
    public SeasonalScenarioRequestStatus Status { get; set; } = status;

    /// <summary>Starts a seasonal scenario request in status created.</summary>
    public static SeasonalScenarioRequest NewSeasonalScenarioRequest(string code, string label)
    {
        return new SeasonalScenarioRequest(
            code,
            label,
            "",
            false,
            0.0,
            "",
            false,
            0.0,
            "",
            0,
            SeasonalScenarioRequestStatus.Created);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case SeasonalScenarioRequestStatus.Retired:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(SeasonalScenarioRequestStatus next)
    {
        switch (Status)
        {
            case SeasonalScenarioRequestStatus.Created:
                return next == SeasonalScenarioRequestStatus.Validated || next == SeasonalScenarioRequestStatus.Published;
            case SeasonalScenarioRequestStatus.Validated:
                switch (next)
                {
                    case SeasonalScenarioRequestStatus.Published:
                        return true;
                }
                return false;
            case SeasonalScenarioRequestStatus.Published:
                switch (next)
                {
                    case SeasonalScenarioRequestStatus.Retired:
                        return true;
                }
                return false;
        }
        return false;
    }

    public void MoveTo(SeasonalScenarioRequestStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"seasonal scenario request {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string DisplayLabel()
    {
        return $"{Code}: {Label}";
    }

    public double WeightedValue()
    {
        return Math.Round(Fte + Ratio, 2);
    }

    public SeasonalScenarioRequest Copy()
    {
        return new SeasonalScenarioRequest(
            Code,
            Label,
            Description,
            Approved,
            Ratio,
            Region,
            Archived,
            Fte,
            Grade,
            Priority,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Region);
    }
}
