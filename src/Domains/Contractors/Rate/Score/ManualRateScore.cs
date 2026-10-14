namespace Acme.Workforce.Contractors.Rate.Score;

using Acme.Workforce.Kit.Text;

/// <summary>Where a manual rate score is in its life cycle.</summary>
public enum ManualRateScoreStatus
{
    Proposed,
    Approved,
    Rejected,
    Withdrawn,
}

/// <summary>The record of a manual rate score and its current status.</summary>
public sealed class ManualRateScore(string code, string name, int retries, double floorValue, Dictionary<string, string> attributes, string notes, int priority, double fte, int attempts, string country, ManualRateScoreStatus status)
{
    public string Code { get; set; } = code;
    public string Name { get; set; } = name;
    public int Retries { get; set; } = retries;
    public double FloorValue { get; set; } = floorValue;
    public Dictionary<string, string> Attributes { get; set; } = attributes;
    public string Notes { get; set; } = notes;
    public int Priority { get; set; } = priority;
    public double Fte { get; set; } = fte;
    public int Attempts { get; set; } = attempts;
    public string Country { get; set; } = country;
    public ManualRateScoreStatus Status { get; set; } = status;

    /// <summary>Starts a manual rate score in status proposed.</summary>
    public static ManualRateScore NewManualRateScore(string code, string name)
    {
        return new ManualRateScore(
            code,
            name,
            0,
            0.0,
            new Dictionary<string, string>(),
            "",
            0,
            0.0,
            0,
            "",
            ManualRateScoreStatus.Proposed);
    }

    /// <summary>Reports whether the record can no longer change status.</summary>
    public bool IsTerminal()
    {
        switch (Status)
        {
            case ManualRateScoreStatus.Withdrawn:
                return true;
        }
        return false;
    }

    public bool CanMoveTo(ManualRateScoreStatus next)
    {
        switch (Status)
        {
            case ManualRateScoreStatus.Proposed:
                switch (next)
                {
                    case ManualRateScoreStatus.Rejected:
                    case ManualRateScoreStatus.Withdrawn:
                        return true;
                }
                return false;
            case ManualRateScoreStatus.Approved:
                return next == ManualRateScoreStatus.Rejected || next == ManualRateScoreStatus.Withdrawn;
            case ManualRateScoreStatus.Rejected:
                return next == ManualRateScoreStatus.Withdrawn;
        }
        return false;
    }

    public void MoveTo(ManualRateScoreStatus next)
    {
        if (!CanMoveTo(next))
        {
            throw new InvalidOperationException($"manual rate score {Code} cannot move from {Status} to {next}");
        }
        Status = next;
    }

    public string Label()
    {
        return $"{Code} / {Name}";
    }

    public string Attribute(string key, string fallback)
    {
        if (!Attributes.TryGetValue(key, out var found))
        {
            return fallback;
        }
        if (found.Trim() == "")
        {
            return fallback;
        }
        return found;
    }

    public double WeightedVolume()
    {
        return Math.Round(FloorValue - (double)Retries, 2);
    }

    public ManualRateScore Copy()
    {
        return new ManualRateScore(
            Code,
            Name,
            Retries,
            FloorValue,
            Attributes,
            Notes,
            Priority,
            Fte,
            Attempts,
            Country,
            Status);
    }

    public string GroupKey()
    {
        return Text.Normalize(Country);
    }
}
