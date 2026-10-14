namespace Acme.Workforce.Orgdesign.Scenario.Group;

using Acme.Workforce.Kit.Text;

/// <summary>Where a weighted scenario group is in its life cycle.</summary>
public enum WeightedScenarioGroupStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A weighted scenario group. Its code is unique within a tenant.</summary>
public sealed class WeightedScenarioGroup(string code, string title, double fte, string badge, string location, double maximum, WeightedScenarioGroupStatus status)
{
    public string Code { get; set; } = code;
    public string Title { get; set; } = title;
    public double Fte { get; set; } = fte;
    public string Badge { get; set; } = badge;
    public string Location { get; set; } = location;
    public double Maximum { get; set; } = maximum;
    public WeightedScenarioGroupStatus Status { get; set; } = status;

    /// <summary>Starts a weighted scenario group in status new.</summary>
    public static WeightedScenarioGroup NewWeightedScenarioGroup(string code, string title)
    {
        return new WeightedScenarioGroup(code, title, 0.0, "", "", 0.0, WeightedScenarioGroupStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case WeightedScenarioGroupStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(WeightedScenarioGroupStatus next)
    {
        switch (Status)
        {
            case WeightedScenarioGroupStatus.New:
                return next == WeightedScenarioGroupStatus.Failed;
            case WeightedScenarioGroupStatus.Queued:
                switch (next)
                {
                    case WeightedScenarioGroupStatus.Failed:
                        return true;
                }
                return false;
            case WeightedScenarioGroupStatus.Running:
                switch (next)
                {
                    case WeightedScenarioGroupStatus.Done:
                    case WeightedScenarioGroupStatus.Failed:
                        return true;
                }
                return false;
            case WeightedScenarioGroupStatus.Done:
                return next == WeightedScenarioGroupStatus.Failed;
        }
        return false;
    }

    public void MoveTo(WeightedScenarioGroupStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"weighted scenario group {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} ({Title})";
    }

    public double NetValue()
    {
        return Math.Round(Maximum - Fte, 2);
    }

    public WeightedScenarioGroup Copy()
    {
        return new WeightedScenarioGroup(Code, Title, Fte, Badge, Location, Maximum, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Title);
    }
}
