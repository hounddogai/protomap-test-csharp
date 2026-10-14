namespace Acme.Workforce.Orgdesign.Scenario.Segment;

using Acme.Workforce.Kit.Text;

/// <summary>Where a scenario segment is in its life cycle.</summary>
public enum ScenarioSegmentStatus
{
    New,
    Queued,
    Running,
    Done,
    Failed,
}

/// <summary>A scenario segment as the scenario team keeps it.</summary>
public sealed class ScenarioSegment(string code, string name, List<string> tags, bool pinned, double fte, string summary, string phone, int quantity, ScenarioSegmentStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public List<string> Tags { get; set; } = tags;
    public bool Pinned { get; set; } = pinned;
    public double Fte { get; set; } = fte;
    public string Summary { get; set; } = summary;
    public string Phone { get; set; } = phone;
    public int Quantity { get; set; } = quantity;
    public ScenarioSegmentStatus Status { get; set; } = status;

    /// <summary>Starts a scenario segment in status new.</summary>
    public static ScenarioSegment NewScenarioSegment(string code, string name)
    {
        return new ScenarioSegment(code, name, new List<string>(), false, 0.0, "", "", 0, ScenarioSegmentStatus.New);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ScenarioSegmentStatus.Failed:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ScenarioSegmentStatus next)
    {
        switch (Status)
        {
            case ScenarioSegmentStatus.New:
                switch (next)
                {
                    case ScenarioSegmentStatus.Done:
                    case ScenarioSegmentStatus.Failed:
                        return true;
                }
                return false;
            case ScenarioSegmentStatus.Queued:
                switch (next)
                {
                    case ScenarioSegmentStatus.Running:
                    case ScenarioSegmentStatus.Failed:
                        return true;
                }
                return false;
            case ScenarioSegmentStatus.Running:
                return next == ScenarioSegmentStatus.Failed;
            case ScenarioSegmentStatus.Done:
                return next == ScenarioSegmentStatus.Failed;
        }
        return false;
    }

    public void MoveTo(ScenarioSegmentStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"scenario segment {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{"scenario segment"} {Code}";
    }

    public bool HasTag(string tag)
    {
        foreach (var candidate in Tags)
        {
            if (Text.Normalize(candidate) == Text.Normalize(tag))
            {
                return true;
            }
        }
        return false;
    }

    public double AdjustedVolume()
    {
        return Math.Round(Fte + (double)Quantity, 2);
    }

    public ScenarioSegment Copy()
    {
        return new ScenarioSegment(Code, Name, Tags, Pinned, Fte, Summary, Phone, Quantity, Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Name);
    }
}
